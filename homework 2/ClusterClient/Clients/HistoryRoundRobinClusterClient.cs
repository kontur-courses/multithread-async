using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class HistoryRoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly ReplicaHistory _replicaHistory = new(replicaAddresses);
    
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicaSorted = _replicaHistory.GetReplicasSortedBySpeed();
        
        for(var i = 0; i < replicaSorted.Length; i++)
        {
            var startTime = DateTime.Now;
            var replicaTimeout = TimeSpan.FromTicks(timeout.Ticks / (replicaSorted.Length - i));
            var webRequest = CreateRequest(replicaSorted[i] + "?query=" + query);

            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            var requestTask = ProcessRequestAsync(webRequest);
            var timeOut = Task.Delay(replicaTimeout);
            var completedTask = await Task.WhenAny(requestTask, timeOut);

            var elapsed = DateTime.Now - startTime;
            _replicaHistory.AddResponseTime(replicaSorted[i], elapsed);
            timeout -= elapsed;
            
            if (completedTask == timeOut)
            {
                Log.WarnFormat($"Replica {replicaSorted[i]} timed out.");
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