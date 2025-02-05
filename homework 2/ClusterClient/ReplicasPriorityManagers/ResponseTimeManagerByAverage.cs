using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ClusterClient.ReplicasPriorityManagers
{
    public class ResponseTimeManagerByAverage : IReplicasPriorityManager
    {
        private readonly ConcurrentDictionary<string, ReplicaStatistics> replicasStats
            = new ConcurrentDictionary<string, ReplicaStatistics>();

        public Dictionary<string, ReplicaStatistics> GetReplicasStats
            => replicasStats.ToDictionary();

        public void SetReplicaStatsTime(string replicaAddress, TimeSpan replicaWorkingTime)
            => replicasStats
                .AddOrUpdate(
                    replicaAddress,
                    _ =>
                    {
                        var newReplica = new ReplicaStatistics();
                        newReplica.Set(replicaWorkingTime);
                        return newReplica;
                    },
                    (_, oldValue) =>
                    {
                        oldValue.Set(replicaWorkingTime);
                        return oldValue;
                    });

        public string[] SortReplicasAddresses(string[] replicaAddresses)
            => replicaAddresses
                .OrderBy(
                    address => replicasStats
                        .GetValueOrDefault(address, new ReplicaStatistics()).GetAverageResponseTime())
                .ToArray();

        public void AddToReplicaStatsTime(string replicaAddress, TimeSpan replicaWorkingTime)
            => replicasStats
                .AddOrUpdate(
                    replicaAddress,
                    _ =>
                    {
                        var newReplica = new ReplicaStatistics();
                        newReplica.Set(replicaWorkingTime);
                        return newReplica;
                    },
                    (_, oldValue) =>
                    {
                        oldValue.Add(replicaWorkingTime);
                        return oldValue;
                    });
    }
}
