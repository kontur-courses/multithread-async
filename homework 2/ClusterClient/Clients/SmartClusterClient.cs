using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using TaskExtensions = ClusterClient.Extensions.TaskExtensions;

namespace ClusterClient.Clients;

public class SmartClusterClient : ClusterClientBase
{
    public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var requestTasks = new List<Task<string>>();
        var badRequestCount = 0;

        foreach (var request in ReplicaAddresses.Select(x => CreateRequest(x, query)))
        {
            var requestTimeout = timeout / (ReplicaAddresses.Length - badRequestCount);
            var timeoutTask = TaskExtensions.CreateTaskDelayWithResult<string>(requestTimeout);

            Log.InfoFormat($"Processing request {request.RequestUri}");

            var newRequestTask = ProcessRequestAsync(request);
            requestTasks.Add(newRequestTask);
            var finishedTask = await Task.WhenAny(requestTasks.Append(timeoutTask));

            if (finishedTask == timeoutTask)
                continue;

            if (finishedTask.IsCompletedSuccessfully)
                return await finishedTask;

            requestTasks.Remove(finishedTask);
            badRequestCount++;
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}