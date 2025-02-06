using System;
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
            var replicaCount = ReplicaAddresses.Length;
            foreach (var address in ReplicaAddresses)
            {
                var perRequestTimeout = TimeSpan.FromMilliseconds(timeout.Ticks / replicaCount);
                var request = CreateRequest(address + $"?query={query}");
                Log.InfoFormat($"Processing {request.RequestUri}");
            
                var requestTask = ProcessRequestAsync(request);
                var timeOut = Task.Delay(perRequestTimeout);
                var completedTask = await Task.WhenAny(requestTask, timeOut);
            
                if (completedTask == requestTask)
                    return await requestTask;
                
                replicaCount--;
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
