using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient(string[] replicaAddresses)
        : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            Log.InfoFormat($"Start processing query \"{query}\" with {timeout} timeout");

            var stopwatch = new Stopwatch();
            var replicasLeftCount = ReplicaAddresses.Length;
            foreach (var address in ReplicaAddresses)
            {
                stopwatch.Restart();

                var request = CreateRequest($"{address}?query={query}");
                Log.InfoFormat($"Processing {request.RequestUri}");

                var timeoutTask = Task.Delay(timeout / replicasLeftCount);
                var requestTask = ProcessRequestAsync(request);

                await Task.WhenAny(requestTask, timeoutTask);
                stopwatch.Stop();

                if (requestTask.IsFaulted)
                {
                    timeout = timeout.Subtract(stopwatch.Elapsed);
                    replicasLeftCount -= 1;
                    continue;
                }

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
