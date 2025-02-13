using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly Dictionary<string, long> replicaStatistics = Enumerable.Range(0, replicaAddresses.Length)
        .Select(i => (replicaAddresses[i], 0L)).ToDictionary();

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasks = GetOrderedReplicasByFasterRespond()
            .Select((uri, index) =>
            {
                var request = CreateRequest($"{uri}?query={query}");
                return (TryProcessRequestAsync(request), index);
            });
        var previousTasks = new List<Task<string>>();

        foreach (var (task, index) in tasks)
        {
            previousTasks.Add(task);
            var previousTask = Task.WhenAny(previousTasks);
            var singleTimeout = timeout / (ReplicaAddresses.Length - index);
            var timer = Stopwatch.StartNew();

            await Task.WhenAny(previousTask, Task.Delay(singleTimeout));

            timer.Stop();
            timeout -= TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);

            replicaStatistics[ReplicaAddresses[index]] = timer.ElapsedMilliseconds;

            if (!previousTask.IsCompleted)
            {
                continue;
            }

            if (previousTask.Result.Result is not null)
            {
                return previousTask.Result.Result;
            }

            previousTasks.Remove(previousTask.Result);
        }

        throw new TimeoutException();
    }

    private IEnumerable<string> GetOrderedReplicasByFasterRespond()
    {
        return replicaStatistics.OrderBy(t => t.Value)
            .Select(y => y.Key);
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}