using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        using var token = new CancellationTokenSource(timeout);
        var tasks = ReplicaAddresses
            .Select(r => ProcessRequestAsync(r, query, token.Token, timeout))
            .ToList();

        while (tasks.Count > 0)
        {
            var result = await Task.WhenAny(tasks);
            tasks.Remove(result);
            if (result.IsCompletedSuccessfully)
            {
                token.Cancel();
                return result.Result;
            }
            if (result.IsFaulted)
                Log.Error("Task failed", result.Exception);
        }

        throw new TimeoutException("Task timed out");
    }

    private async Task<string> ProcessRequestAsync(string replicaAddress, string query, CancellationToken token, TimeSpan timeout)
    {
        if (token.IsCancellationRequested)
            return await Task.FromCanceled<string>(token);
        var request = CreateRequest(replicaAddress + "?query=" + query);
        Log.InfoFormat($"Processing {request.RequestUri}");
        var requestTask = ProcessRequestAsync(request);
        await Task.WhenAny(requestTask, Task.Delay(timeout));
        if(!requestTask.IsCompleted)
            throw new TimeoutException("Task timed out");
        return await requestTask;
    }


    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
}