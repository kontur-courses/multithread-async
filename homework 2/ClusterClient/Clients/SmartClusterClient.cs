using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Clients.Extensions;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient(string[] replicaAddresses)
        : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            using var cancellation = new CancellationTokenSource(timeout);
            Log.InfoFormat($"Start processing query \"{query}\" with {timeout} timeout");

            var stopwatch = new Stopwatch();

            var replicasLeftCount = ReplicaAddresses.Length;
            var tasks = new List<Task<string>>(ReplicaAddresses.Length);
            foreach (var address in ReplicaAddresses)
            {
                stopwatch.Restart();

                var request = CreateRequest($"{address}?query={query}");
                Log.InfoFormat($"Processing {request.RequestUri}");

                var timeoutTask = Task.Delay(timeout / replicasLeftCount);
                var requestTask = Task.Run(async () =>
                {
                    cancellation.Token.Register(request.Abort);
                    return await ProcessRequestAsync(request);
                }, cancellation.Token);
                tasks.Add(requestTask);

                var completedTask = await Task.WhenAny(tasks.Concat(new Task[] { timeoutTask }));
                stopwatch.Stop();

                if (completedTask.IsFaulted)
                {
                    timeout = timeout.Subtract(stopwatch.Elapsed);
                    replicasLeftCount -= 1;
                    continue;
                }

                if (completedTask != timeoutTask && completedTask.IsCompletedSuccessfully)
                {
                    await cancellation.CancelAsync();
                    return ((Task<string>)completedTask).Result;
                }
            }

            var timeoutMessage = $"No positive response received for query \"{query}\" with {timeout} timeout";
            return await tasks.WaitForFirstSuccessAsync(timeoutMessage, cancellation);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
