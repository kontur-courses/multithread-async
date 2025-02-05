using System;
using System.Linq;

namespace ClusterClient.Clients;

public abstract class ClusterClientWithHistory(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly ReplicaInfo[] _replicaStatistics = Enumerable.Range(0, replicaAddresses.Length)
        .Select(i => new ReplicaInfo(replicaAddresses[i], long.MaxValue)).ToArray();

    protected void ReorderReplicas(string replicaName, long newReplicaTime)
    {
        lock (_replicaStatistics)
        {
            var replicaInfo = _replicaStatistics.FirstOrDefault(x => x.Name == replicaName);
            var replicaPos = Array.IndexOf(_replicaStatistics, replicaInfo);
            
            _replicaStatistics[replicaPos] = new ReplicaInfo(replicaName, newReplicaTime);
            Array.Sort(_replicaStatistics, 
                (first, second) => first.Time.CompareTo(second.Time));
        }
    }

    protected string[] OrderedReplicas()
    {
        lock (_replicaStatistics)
        {
            return _replicaStatistics.Select(x => x.Name).ToArray();
        }
    }
    
}