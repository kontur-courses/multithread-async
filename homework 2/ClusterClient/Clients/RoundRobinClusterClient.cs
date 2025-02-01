﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var requestStopwatch = new Stopwatch();
        var replicaCount = ReplicaAddresses.Length;
        requestStopwatch.Start();
        foreach (var replicaAddress in ReplicaAddresses)
        {
            var remainingTime = timeout - requestStopwatch.Elapsed;
            var actualTimeout = remainingTime / replicaCount;

            var uri = BuildUri(replicaAddress, query);
            var webRequest = CreateRequest(uri);

            var result = await ProcessItemRequestOrNullAsync(webRequest, actualTimeout, cancellationToken);
            if (result != null)
            {
                return result;
            }

            replicaCount--;
        }

        throw new TimeoutException("Запрос превысил время ожидания.");
    }

    private async Task<string?> ProcessItemRequestOrNullAsync(WebRequest webRequest, TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var requestToken = requestCts.Token;
        Log.InfoFormat($"Processing {webRequest.RequestUri}");
        var resultTask = ProcessRequestAsync(webRequest, requestToken);
        var completedTask = await Task.WhenAny(resultTask, Task.Delay(timeout, requestToken));
        await requestCts.CancelAsync();
        if (completedTask == resultTask && resultTask.IsCompletedSuccessfully)
        {
            return resultTask.Result;
        }

        return null;
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}