using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var requestStopwatch = new Stopwatch();
        var replicaCount = ReplicaAddresses.Length;
        var tasks = new List<Task<string>>();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedToken = linkedCts.Token;

        requestStopwatch.Start();
        foreach (var replicaAddress in ReplicaAddresses)
        {
            var remainingTime = timeout - requestStopwatch.Elapsed;
            var actualTimeout = remainingTime / replicaCount;

            var uri = BuildUri(replicaAddress, query);
            var webRequest = CreateRequest(uri);
            Log.InfoFormat($"Processing {webRequest.RequestUri}");
            tasks.Add(ProcessRequestAsync(webRequest, linkedToken));

            var result = await ProcessItemRequestOrNullAsync(tasks, actualTimeout, cancellationToken);
            if (result != null)
            {
                await linkedCts.CancelAsync();
                return result;
            }

            replicaCount--;
        }

        throw new TimeoutException("Запрос превысил время ожидания.");
    }

    private static async Task<string?> ProcessItemRequestOrNullAsync(IEnumerable<Task<string>> tasks, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var firstSuccessTask = tasks.GetFirstSuccessTask(cancellationToken);
        var completedTask = await Task.WhenAny(firstSuccessTask, Task.Delay(timeout, cancellationToken));
        if (completedTask == firstSuccessTask && firstSuccessTask.IsCompletedSuccessfully)
        {
            var firstSuccessResult = await firstSuccessTask;
            var result = await firstSuccessResult;
            return result;
        }

        return null;
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}