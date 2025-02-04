using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var activeRequests = new List<Task<string>>();
        
        for(var i = 0; i < ReplicaAddresses.Length; i++)
        {
            var startTime = DateTime.Now;
            var replicaTimeout = TimeSpan.FromTicks(timeout.Ticks / (ReplicaAddresses.Length - i));
            var webRequest = CreateRequest(ReplicaAddresses[i] + "?query=" + query);
            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            var requestTask = ProcessRequestAsync(webRequest);
            var timeOut = Task.Delay(replicaTimeout);
            activeRequests.Add(requestTask);

            var ongoingTasks = Task.WhenAny(activeRequests);
            var completedTask = await Task.WhenAny(ongoingTasks, timeOut);
            timeout -= DateTime.Now - startTime;
            
            if (completedTask == timeOut)
            {
                Log.WarnFormat($"Replica {ReplicaAddresses[i]} timed out.");
            }
            else if (ongoingTasks.IsCompletedSuccessfully)
            {
                var task = ongoingTasks.Result;
                if (task.IsCompletedSuccessfully)
                {
                    return task.Result;
                }
                activeRequests.Remove(task);
            }
        }
            
        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}