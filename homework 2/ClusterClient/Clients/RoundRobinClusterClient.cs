using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
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

        foreach (var (task, index) in tasks)
        {
            var singleTimeout = timeout / (ReplicaAddresses.Length - index);
            var timer = Stopwatch.StartNew();

            await Task.WhenAny(task, Task.Delay(singleTimeout));

            timer.Stop();
            timeout -= TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);

            replicaStatistics[ReplicaAddresses[index]] = timer.ElapsedMilliseconds;

            if (task.IsCompleted && task.Result is not null)
            {
                return task.Result;
            }
        }

        throw new TimeoutException();
    }

    private IEnumerable<string> GetOrderedReplicasByFasterRespond()
    {
        return replicaStatistics.OrderBy(t => t.Value)
            .Select(y => y.Key);
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}