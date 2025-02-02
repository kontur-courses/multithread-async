using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly StatisticsCollector _statsCollector = new();
    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));

    private record ReplicaTask(
        Task<string> Task,
        string Replica,
        Stopwatch Stopwatch);

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicas = _statsCollector.GetSortedReplicas(ReplicaAddresses);
        var perReplicaTimeout = CalculateReplicaTimeout(timeout, replicas.Count);

        var overallStopwatch = Stopwatch.StartNew();
        return await ExecuteRequestStrategy(query, timeout, replicas, perReplicaTimeout, overallStopwatch);
    }

    private async Task<string> ExecuteRequestStrategy(
        string query,
        TimeSpan timeout,
        IReadOnlyList<string> replicas,
        TimeSpan perReplicaTimeout,
        Stopwatch overallStopwatch)
    {
        var pendingTasks = new List<ReplicaTask>();
        Exception lastException = null;
        var replicaIndex = 0;

        while (overallStopwatch.Elapsed < timeout)
        {
            if (replicaIndex < replicas.Count)
            {
                var replicaTask = CreateReplicaTask(query, replicas[replicaIndex]);
                pendingTasks.Add(replicaTask);
                replicaIndex++;
            }

            var result = await WaitForNextCompletion(pendingTasks, perReplicaTimeout);
            if (result.IsSuccess)
            {
                return result.Value;
            }

            if (result.Exception != null)
            {
                lastException = result.Exception;
            }
        }

        return await ProcessRemainingTasks(pendingTasks)
               ?? throw lastException
                        ?? throw new TimeoutException("All replicas timed out");
    }

    private ReplicaTask CreateReplicaTask(string query, string replica)
    {
        var uri = BuildRequestUri(replica, query);
        var webRequest = CreateRequest(uri);
        var stopwatch = Stopwatch.StartNew();
        var task = ProcessRequestAsync(webRequest);

        return new ReplicaTask(task, replica, stopwatch);
    }

    private async Task<(bool IsSuccess, string Value, Exception Exception)> WaitForNextCompletion(
        ICollection<ReplicaTask> pendingTasks,
        TimeSpan perReplicaTimeout)
    {
        var delayTask = Task.Delay(perReplicaTimeout);
        var tasksToWait = pendingTasks.Select(x => x.Task).Cast<Task>().Append(delayTask);

        var finished = await Task.WhenAny(tasksToWait);
        if (finished == delayTask)
        {
            return (false, null, null);
        }

        var completedTask = pendingTasks.FirstOrDefault(x => x.Task == finished);
        if (completedTask == null)
        {
            return (false, null, null);
        }

        pendingTasks.Remove(completedTask);

        try
        {
            var result = await completedTask.Task;
            UpdateReplicaStats(completedTask);
            return (true, result, null);
        }
        catch (Exception ex)
        {
            UpdateReplicaStats(completedTask);
            return (false, null, ex);
        }
    }

    private async Task<string> ProcessRemainingTasks(List<ReplicaTask> pendingTasks)
    {
        foreach (var task in pendingTasks.Where(t => t.Task.IsCompleted))
        {
            try
            {
                var result = await task.Task;
                UpdateReplicaStats(task);
                return result;
            }
            catch (Exception)
            {
                UpdateReplicaStats(task);
            }
        }

        return null;
    }

    private void UpdateReplicaStats(ReplicaTask replicaTask)
    {
        replicaTask.Stopwatch.Stop();
        _statsCollector.UpdateStats(replicaTask.Replica, replicaTask.Stopwatch.ElapsedMilliseconds);
    }

    private static string BuildRequestUri(string replica, string query)
        => $"{replica}?query={query}";

    private static TimeSpan CalculateReplicaTimeout(TimeSpan totalTimeout, int replicaCount)
        => TimeSpan.FromMilliseconds(totalTimeout.TotalMilliseconds / replicaCount);
}