using System;
using System.Linq;

namespace ClusterClient.Clients;

public abstract class ClusterClientWithHistory(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private ReplicaInfo[] replicaseStatistics = Enumerable.Range(0, replicaAddresses.Length)
        .Select(i => new ReplicaInfo(long.MaxValue, replicaAddresses[i])).ToArray();
    
    protected void ReorderReplicas(long elapsedSeconds, string replicaName)
    {
        lock (replicaseStatistics)
        {
            var replicaInfo = replicaseStatistics
                .First(info => info.Name == replicaName);
            var replicaPos = Array.IndexOf(replicaseStatistics, replicaInfo);
        
            replicaseStatistics[replicaPos] = new ReplicaInfo(elapsedSeconds, replicaName);
            Array.Sort(replicaseStatistics, (info1, info2) => info1.Speed.CompareTo(info2.Speed));
        }
    }

    protected string[] OrderedReplicas()
    {
        lock (replicaseStatistics)
        {
            return replicaseStatistics.Select(info => info.Name).ToArray();
        }
    }
    // => replicaseStatistics.Select(info => info.Name).ToArray();
    
    private record ReplicaInfo(long Speed, string Name);
}