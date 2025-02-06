using System;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace ClusterClient.Clients;

public abstract class ClusterClientBaseWithHistory(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly ReplicaInfo[] _replicasStatistics = Enumerable
        .Range(0, replicaAddresses.Length)
        .Select(i => new ReplicaInfo(long.MaxValue, replicaAddresses[i]))
        .ToArray();

    protected override void PostProcessRequest(WebRequest request, Stopwatch timer)
    {
        base.PostProcessRequest(request, timer);
        ReorderReplicas(timer.ElapsedMilliseconds, RequestPath(request));
    }

    private void ReorderReplicas(long elapsedSeconds, string replicaName)
    {
        lock (_replicasStatistics)
        {
            var replicaInfo = _replicasStatistics
                .First(info => info.Name == replicaName);
            var replicaIdx = Array.IndexOf(_replicasStatistics, replicaInfo);
        
            _replicasStatistics[replicaIdx] = new ReplicaInfo(elapsedSeconds, replicaName);
            Array.Sort(_replicasStatistics, (info1, info2) => info1.Speed.CompareTo(info2.Speed));
        }
    }

    protected string[] OrderedReplicas()
    {
        lock (_replicasStatistics)
        {
            return _replicasStatistics.Select(info => info.Name).ToArray();
        }
    }

    private static string RequestPath(WebRequest request)
    {
        var uri = request.RequestUri;
        return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
    }
    
    private record ReplicaInfo(long Speed, string Name);
}