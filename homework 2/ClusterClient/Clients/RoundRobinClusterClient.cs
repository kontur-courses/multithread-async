using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        for(var i = 0; i < ReplicaAddresses.Length; i++)
        {
            var startTime = DateTime.Now;
            var perReplicaTimeout = TimeSpan.FromTicks(timeout.Ticks / (ReplicaAddresses.Length - i));
            var webRequest = CreateRequest(ReplicaAddresses[i] + "?query=" + query);

            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            var requestTask = ProcessRequestAsync(webRequest);
            var timeOut = Task.Delay(perReplicaTimeout);
            var completedTask = await Task.WhenAny(requestTask, timeOut);
            timeout -= DateTime.Now - startTime;
            
            if (completedTask == timeOut)
            {
                Log.WarnFormat($"Replica {ReplicaAddresses[i]} timed out.");
            }
            else if (requestTask.IsCompletedSuccessfully)
            {
                return requestTask.Result;
            }
        }
        
        throw new TimeoutException();
    }
    
    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}