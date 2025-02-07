using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient : ClusterClientBase
{
    public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicaCount = ReplicaAddresses.Length;
        var requests = ReplicaAddresses
            .Select(uri => CreateRequest($"{uri}?query={query}"));
        var requestTasks = new List<Task<string>>();
        foreach (var request in requests)
        {
            var perRequestTimeout = timeout / replicaCount;
            
            var timeoutTask = Task.Delay(perRequestTimeout);
            var requestTask = ProcessRequestAsync(request);
            Log.InfoFormat($"Processing request {request.RequestUri}");
            
            requestTasks.Add(requestTask);
            var completedTask = await Task.WhenAny(requestTasks.Append(timeoutTask));

            if (timeoutTask == completedTask)
                continue;

            if (completedTask.IsCompletedSuccessfully)
                return await (Task<string>)completedTask;
            requestTasks.Remove((Task<string>)completedTask);
            replicaCount--;
        }
        throw new TimeoutException();
    }
    
    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}