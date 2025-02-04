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
            var webRequests = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query));
            var replicaCount = ReplicaAddresses.Length;
            
            foreach (var request in webRequests)
            {
                var localTimeout = timeout / replicaCount;
                
                Log.InfoFormat($"Processing {request.RequestUri}");
                
                var requestTask = ProcessRequestAsync(request);
                var completedTask = await Task.WhenAny(requestTask, Task.Delay(localTimeout));

                if (!completedTask.IsCompletedSuccessfully)
                {
                    replicaCount--;
                    continue;
                }

                if (requestTask == completedTask)
                    return await requestTask;
            } 
            throw new TimeoutException();
        }
        
        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
