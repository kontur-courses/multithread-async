using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using ClusterHistory.Interfaces;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient : ClusterClientBase
{
    public ParallelClusterClient(string[] replicaAddresses, IReplicaSendHistory replicaSendHistory)
        : base(replicaAddresses, replicaSendHistory)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedToken = linkedCts.Token;
        var webRequests = ReplicaAddresses.Select(r => CreateUri(r, query)).Select(CreateRequest);
        var requestTasks = webRequests.Select(x =>
        {
            Log.InfoFormat($"Processing {x.RequestUri}");
            return ProcessRequestAsync(x, linkedToken);
        });
        var resultTask = await requestTasks.WhenAnyFirstSuccessfulTask(timeout, linkedToken);
        await linkedCts.CancelAsync();
        return await resultTask;
    }

    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
}