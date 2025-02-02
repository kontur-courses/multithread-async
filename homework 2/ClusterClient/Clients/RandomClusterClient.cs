using System;
using System.Threading;
using System.Threading.Tasks;
using ClusterHistory.Interfaces;
using log4net;

namespace ClusterClient.Clients;

public class RandomClusterClient : ClusterClientBase
{
    private readonly Random random = new();

    public RandomClusterClient(string[] replicaAddresses, IReplicaSendHistory replicaSendHistory)
        : base(replicaAddresses, replicaSendHistory)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedToken = linkedCts.Token;
        var replicaAddresses = ReplicaAddresses[random.Next(ReplicaAddresses.Length)];
        var uri = CreateUri(replicaAddresses, query);
        var webRequest = CreateRequest(uri);
        Log.InfoFormat($"Processing {webRequest.RequestUri}");
        var resultTask = ProcessRequestAsync(webRequest, linkedToken);
        await Task.WhenAny(resultTask, Task.Delay(timeout, linkedToken));
        if (!resultTask.IsCompleted)
        {
            await linkedCts.CancelAsync();
            throw new TimeoutException("Запрос превысил время ожидания.");
        }

        return resultTask.Result;
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
}