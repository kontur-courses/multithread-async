using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ClusterClient.ReplicasPriorityManagers;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinWithPriorityClusterClient(
        string[] replicaAddresses,
        IReplicasPriorityManager replicasPriorityManager)
        : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            Log.InfoFormat($"Start processing query \"{query}\" with {timeout} timeout");

            var replicasOrder = replicasPriorityManager.SortReplicasAddresses(ReplicaAddresses);
            var replicasLeftCount = replicasOrder.Length;

            foreach (var address in replicasOrder)
            {
                var stopwatch = Stopwatch.StartNew();

                var request = CreateRequest($"{address}?query={query}");
                Log.InfoFormat($"Processing {request.RequestUri}");

                var timeoutTask = Task.Delay(timeout / replicasLeftCount);
                var requestTask = ProcessRequestAsync(request);

                await Task.WhenAny(requestTask, timeoutTask);
                var workingTime = stopwatch.Elapsed;

                if (requestTask.IsFaulted)
                {
                    replicasPriorityManager.SetReplicaStatsTime(address, TimeSpan.MaxValue);
                    timeout = timeout.Subtract(workingTime);
                    replicasLeftCount -= 1;
                    continue;
                }

                replicasPriorityManager.AddToReplicaStatsTime(address, workingTime);
                if (requestTask.IsCompletedSuccessfully)
                {
                    return requestTask.Result;
                }
            }

            var timeoutMessage = $"No positive response received for query \"{query}\" with {timeout} timeout";
            throw new TimeoutException(timeoutMessage);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
