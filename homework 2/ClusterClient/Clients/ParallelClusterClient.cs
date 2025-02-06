using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var cancellationToken = cts.Token;

        var tasks = ReplicaAddresses
            .Select(uri => CreateRequest(uri + "?query=" + query))
            .Select(async request =>
            {
                Log.InfoFormat($"Processing {request.RequestUri}");
                return await ProcessRequestAsync(request).RunTaskWithCancellation(cancellationToken);
            })
            .ToHashSet();
        
        while (tasks.Count != 0)
        {
            var task = await Task.WhenAny(tasks);
            tasks.Remove(task);
            if (task.IsCanceled) throw new TimeoutException("Task timed out");
            if (!task.IsCompletedSuccessfully) continue;
            await cts.CancelAsync();
            return task.Result;
        }

        throw new TimeoutException("All requests failed");
    }

    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
}