using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ClusterClient.Clients;

    public class StatisticsCollector
    {
        private readonly ConcurrentDictionary<string, ReplicaStats> _replicaStats = new();
        public void UpdateStats(string replica, double elapsedMilliseconds)
        {
            _replicaStats.AddOrUpdate(
                replica,
                new ReplicaStats { TotalTime = elapsedMilliseconds, Count = 1 },
                (key, existingStats) =>
                {
                    existingStats.TotalTime += elapsedMilliseconds;
                    existingStats.Count++;
                    return existingStats;
                });
        }

        public List<string> GetSortedReplicas(IEnumerable<string> replicas)
        {
            return replicas.OrderBy(replica => _replicaStats.TryGetValue(replica, out var stats) ? stats.Average :
                0.0).ToList();
        }
    }