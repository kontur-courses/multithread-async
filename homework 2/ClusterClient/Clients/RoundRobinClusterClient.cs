using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();
            var replicasCount = ReplicaAddresses.Length;

            foreach (var uri in ReplicaAddresses)
            {
                var request = CreateRequest($"{uri}?query={query}");
                var taskTimeout = (timeout - stopwatch.Elapsed) / replicasCount;
                replicasCount--;

                var resultTask = ProcessRequestAsync(request);
                var delay = Task.Delay(taskTimeout);
                await Task.WhenAny(resultTask, delay);

                if (resultTask.IsCompletedSuccessfully)
                {
                    return resultTask.Result;
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
