using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RandomClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly Random random = new();

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedToken = linkedCts.Token;
        var uri = ReplicaAddresses[random.Next(ReplicaAddresses.Length)];
        var webRequest = CreateRequest(uri + "?query=" + query);
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