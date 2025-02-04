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

        var tasks = ReplicaAddresses.Select(async replica =>
        {
            var webRequest = CreateRequest(replica + "?query=" + query);

            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            await using var registration = cts.Token.Register(webRequest.Abort);
            return await ProcessRequestAsync(webRequest);
        }).ToList();
        
        while (tasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            if (completedTask.IsCompletedSuccessfully)
            {
                await cts.CancelAsync();
                return completedTask.Result;
            }
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
}