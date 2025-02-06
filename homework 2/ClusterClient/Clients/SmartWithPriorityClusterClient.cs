using ClusterClient.Clients.Extensions;
using ClusterClient.ReplicasPriorityManagers;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Clients
{
    public class SmartWithPriorityClusterClient(
        string[] replicaAddresses,
        IReplicasPriorityManager replicasPriorityManager)
        : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            using var cancellation = new CancellationTokenSource(timeout);
            Log.InfoFormat($"Start processing query \"{query}\" with {timeout} timeout");

            var replicasOrder = replicasPriorityManager.SortReplicasAddresses(ReplicaAddresses);
            var replicasLeftCount = replicasOrder.Length;

            var tasks = new ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)>();
            foreach (var address in replicasOrder)
            {
                var stopwatch = Stopwatch.StartNew();

                var request = CreateRequest($"{address}?query={query}");
                Log.InfoFormat($"Processing {request.RequestUri}");

                var timeoutPerTask = timeout / replicasLeftCount;
                var timeoutTask = Task.Delay(timeoutPerTask);
                var requestTask = Task.Run(async () =>
                {
                    cancellation.Token.Register(request.Abort);
                    return await ProcessRequestAsync(request);
                }, cancellation.Token);
                tasks.TryAdd(requestTask, (address, stopwatch));

                var completedTask = await Task.WhenAny(tasks.Keys.Concat(new Task[] { timeoutTask }));
                if (completedTask == timeoutTask)
                {
                    replicasPriorityManager.AddToReplicaStatsTime(address, timeoutPerTask);
                    continue;
                }

                tasks.TryGetValue((Task<string>)completedTask, out var taskInfo);
                var workingTime = taskInfo.stopwatch.Elapsed;

                if (completedTask.IsFaulted)
                {
                    replicasPriorityManager.SetReplicaStatsTime(taskInfo.address, TimeSpan.MaxValue);
                    timeout = timeout.Subtract(workingTime);
                    replicasLeftCount -= 1;
                    tasks.TryRemove((Task<string>)completedTask, out var removedValue);
                    continue;
                }

                if (completedTask.IsCompletedSuccessfully)
                {
                    await cancellation.CancelAsync();
                    tasks.AddInformationAboutEachTask(replicasPriorityManager);
                    return ((Task<string>)completedTask).Result;
                }
                replicasPriorityManager.AddToReplicaStatsTime(taskInfo.address, workingTime);
            }

            var timeoutMessage = $"No positive response received for query \"{query}\" with {timeout} timeout";
            if (tasks.IsEmpty)
            {
                throw new TimeoutException(timeoutMessage);
            }

            return await tasks.WaitForFirstSuccessAsync(timeoutMessage, cancellation, replicasPriorityManager);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
