using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var devidedTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / ReplicaAddresses.Length);
        var badRequestsCount = 0;
        var tasks = ReplicaAddresses
            .Select(uri => CreateRequest(uri + "?query=" + query))
            .Select(request =>
            {
                Log.InfoFormat($"Processing {request.RequestUri}");
                return ProcessRequestAsync(request);
            });
        var runTasks = new HashSet<Task<string>>();

        foreach (var task in tasks)
        {
            using var cts = new CancellationTokenSource(devidedTimeout);
            runTasks.Add(task);
            var previousTasks = Task.WhenAny(runTasks);
            await Task.WhenAny(previousTasks, Task.Delay(devidedTimeout, cts.Token));
            if (!previousTasks.IsCompleted) 
                continue;
            runTasks.Remove(previousTasks.Result);
            if (previousTasks.Result.IsCompletedSuccessfully)
                return previousTasks.Result.Result;
            if (!previousTasks.Result.IsFaulted) 
                continue;
            badRequestsCount++;
            devidedTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / (ReplicaAddresses.Length - badRequestsCount));
        }
        
        throw new TimeoutException("Task timed out");
        
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}