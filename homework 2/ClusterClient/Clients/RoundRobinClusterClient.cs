using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var timeLimit = timeout;
        var timer = new Stopwatch();
        var totalCount = ReplicaAddresses.Length;
        var requestTimeout = timeLimit / totalCount;

        foreach (var replicaAddress in ReplicaAddresses)
        {
            var requestTask = ProcessRequestAsync(replicaAddress, query);
            var delayTask = Task.Delay(requestTimeout);
            timer.Restart();
            var resultTask = await Task.WhenAny(requestTask, delayTask);
            timer.Stop();
            if (resultTask != delayTask && !resultTask.IsFaulted)
                return await requestTask;
            totalCount -= 1;
            timeLimit -= timer.Elapsed;
            if (totalCount > 1)
                requestTimeout = timeLimit / totalCount;
        }
        throw new TimeoutException();
    }

    private async Task<string> ProcessRequestAsync(string replicaAddress, string query)
    {
        var request = CreateRequest(replicaAddress + "?query=" + query);
        Log.InfoFormat($"Processing {request.RequestUri}");
        return await ProcessRequestAsync(request);
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}