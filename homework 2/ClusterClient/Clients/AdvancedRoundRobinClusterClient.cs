using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class AdvancedRoundRobinClusterClient : ClusterClientBase
{
    public AdvancedRoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    private readonly ConcurrentDictionary<string, long> _replyTimes = new();

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicaTimeout = timeout / ReplicaAddresses.Length;
        var badReplicasTimes = new List<long>();
        var sw = new Stopwatch();

        foreach (var uri in ReplicaAddresses.OrderBy(uri => _replyTimes.GetValueOrDefault(uri, 0)))
        {
            var webRequest = CreateRequest(uri + "?query=" + query);
            
            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            sw.Restart();
            var resultTask = ProcessRequestAsync(webRequest);
            var completed = await Task.WhenAny(resultTask, Task.Delay(replicaTimeout));
            sw.Stop();
            
            _replyTimes.TryAdd(uri, (long)replicaTimeout.TotalMilliseconds);
            
            if (completed == resultTask)
            {
                try
                {
                    var result = resultTask.Result;
                    _replyTimes[uri] = sw.ElapsedMilliseconds;
                    return result;
                }
                catch
                {
                    _replyTimes[uri] = long.MaxValue;
                    
                    badReplicasTimes.Add(sw.ElapsedMilliseconds);
                    replicaTimeout = (timeout - TimeSpan.FromMilliseconds(badReplicasTimes.Sum()))
                                     / (ReplicaAddresses.Length - badReplicasTimes.Count);
                }
            }
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(AdvancedRoundRobinClusterClient));
}