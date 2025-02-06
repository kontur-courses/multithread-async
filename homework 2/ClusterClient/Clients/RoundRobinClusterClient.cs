using System;
using System.Linq;
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
            var requests = ReplicaAddresses
                .Select(uri => CreateRequest($"{uri}?query={query}"));
            foreach (var request in requests)
            {
                var perRequestTimeout = timeout / replicaCount;
                
                var timeoutTask = Task.Delay(perRequestTimeout);
                var requestTask = ProcessRequestAsync(request);
                Log.InfoFormat($"Processing {request.RequestUri}");
                
                var completedTask = await Task.WhenAny(requestTask, timeoutTask);
                if (timeoutTask == completedTask)
                    continue;
                
                if (requestTask.IsCompletedSuccessfully)
                    return await requestTask;
                replicaCount--;
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
