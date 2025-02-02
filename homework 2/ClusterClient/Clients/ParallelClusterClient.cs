using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicaTasks = CreateReplicaTasks(query);
        return await ExecuteReplicaTasksWithTimeout(replicaTasks, timeout);
    }

    private List<Task<string>> CreateReplicaTasks(string query)
    {
        return ReplicaAddresses.Select(replica =>
        {
            var uri = $"{replica}?query={query}";
            var webRequest = CreateRequest(uri);
            return ProcessRequestAsync(webRequest);
        }).ToList();
    }

    private async Task<string> ExecuteReplicaTasksWithTimeout(
        List<Task<string>> replicaTasks,
        TimeSpan timeout)
    {
        using var timeoutCts = new System.Threading.CancellationTokenSource(timeout);
        var timeoutTask = Task.Delay(timeout, timeoutCts.Token);
        Exception lastException = null;

        while (replicaTasks.Count > 0)
        {
            var tasksList = replicaTasks.Select(t => t as Task).Concat(new[] { timeoutTask });
            var completedTask = await Task.WhenAny(tasksList);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException("All replica requests timed out.");
            }

            var completedReplicaTask = replicaTasks.First(t => t == completedTask);
            replicaTasks.Remove(completedReplicaTask);

            try
            {
                var result = await completedReplicaTask;
                timeoutCts.Cancel();
                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        throw lastException ?? new TimeoutException("No replicas responded successfully.");
    }
}