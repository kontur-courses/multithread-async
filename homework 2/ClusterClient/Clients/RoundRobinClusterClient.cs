using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly StatisticsCollector _statsCollector = new();

    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicas = _statsCollector.GetSortedReplicas(ReplicaAddresses);
        var overallStopwatch = Stopwatch.StartNew();

        return await ProcessReplicasSequentially(query, timeout, replicas, overallStopwatch);
    }

    private async Task<string> ProcessReplicasSequentially(
        string query,
        TimeSpan timeout,
        IReadOnlyList<string> replicas,
        Stopwatch overallStopwatch)
    {
        Exception lastException = null;

        for (var i = 0; i < replicas.Count; i++)
        {
            var timeoutInfo = CalculateTimeout(timeout, overallStopwatch, replicas.Count - i);
            if (timeoutInfo.ShouldThrowTimeout)
            {
                throw new TimeoutException("Overall timeout exceeded");
            }

            var replica = replicas[i];
            try
            {
                var result = await ExecuteReplicaRequest(query, replica, timeoutInfo.PerReplicaTimeout);
                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        throw lastException ?? new TimeoutException("All replicas failed to respond");
    }

    private async Task<string> ExecuteReplicaRequest(string query, string replica, TimeSpan replicaTimeout)
    {
        var uri = BuildRequestUri(replica, query);
        var webRequest = CreateRequest(uri);

        var replicaStopwatch = Stopwatch.StartNew();
        try
        {
            var resultTask = ProcessRequestAsync(webRequest);
            var timeoutTask = Task.Delay(replicaTimeout);

            var completedTask = await Task.WhenAny(resultTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                _statsCollector.UpdateStats(replica, replicaTimeout.TotalMilliseconds);
                throw new TimeoutException($"Replica {replica} timed out");
            }

            var result = await resultTask;
            _statsCollector.UpdateStats(replica, replicaStopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception)
        {
            _statsCollector.UpdateStats(replica, replicaStopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    private static string BuildRequestUri(string replica, string query)
        => $"{replica}?query={query}";

    private static (bool ShouldThrowTimeout, TimeSpan PerReplicaTimeout) CalculateTimeout(
        TimeSpan totalTimeout,
        Stopwatch overallStopwatch,
        int remainingReplicas)
    {
        var remainingTimeout = totalTimeout - overallStopwatch.Elapsed;
        if (remainingTimeout <= TimeSpan.Zero)
        {
            return (true, TimeSpan.Zero);
        }

        var perReplicaTimeout = TimeSpan.FromMilliseconds(
            remainingTimeout.TotalMilliseconds / remainingReplicas);

        return (false, perReplicaTimeout);
    }
}