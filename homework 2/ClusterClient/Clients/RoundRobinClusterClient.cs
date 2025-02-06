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
            var addressesCount = ReplicaAddresses.Length;
            var sw = new Stopwatch();
            
            sw.Start();
            foreach (var address in ReplicaAddresses)
            {
                var request = CreateRequest($"{address}?query={query}");
                var timeForRequest = (timeout - sw.Elapsed) / addressesCount;

                var task = ProcessRequestAsync(request);
                await Task.WhenAny(task, Task.Delay(timeForRequest));
                if (task.IsCompletedSuccessfully && task.Result != null)
                    return task.Result;

                addressesCount--;
            }
            
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
