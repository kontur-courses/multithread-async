using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }
    
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var webRequests = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query));
            var backgroundRequests = new List<Task>();
            var replicaCount = ReplicaAddresses.Length;
            
            foreach (var request in webRequests)
            {
                var localTimeout = timeout / replicaCount;
                
                Log.InfoFormat($"Processing {request.RequestUri}");
                
                var requestTask = ProcessRequestAsync(request);
                var timeoutTask = Task.Delay(localTimeout);
                
                backgroundRequests.Add(requestTask);
                backgroundRequests.Add(timeoutTask);

                var completedTask = await Task.WhenAny(backgroundRequests);

                if (!completedTask.IsCompletedSuccessfully)
                {
                    replicaCount--;
                    backgroundRequests.Remove(requestTask);
                    backgroundRequests.Remove(timeoutTask);
                    continue;
                }

                if (timeoutTask != completedTask)
                    return await (Task<string>)completedTask;
                
                backgroundRequests.Remove(timeoutTask);
            } 
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
