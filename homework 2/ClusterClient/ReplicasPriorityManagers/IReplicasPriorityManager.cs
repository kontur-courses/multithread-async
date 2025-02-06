using System;

namespace ClusterClient.ReplicasPriorityManagers
{
    public interface IReplicasPriorityManager
    {
        public string[] SortReplicasAddresses(string[] replicaAddresses);
        public void AddToReplicaStatsTime(string replicaAddress, TimeSpan replicaWorkingTime);
        public void SetReplicaStatsTime(string replicaAddress, TimeSpan replicaWorkingTime);
    }
}
