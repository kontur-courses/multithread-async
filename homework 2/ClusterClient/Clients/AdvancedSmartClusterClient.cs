using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class AdvancedSmartClusterClient : ClusterClientBase
{
    public AdvancedSmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }
    
    private readonly ConcurrentDictionary<string, long> _replyTimes = new();

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicaTimeout = timeout / ReplicaAddresses.Length;
        var taskList = new List<Task<string>>();
        var badReplicasTimes = new List<long>();
        var stopwatches = new Dictionary<Task<string>, Stopwatch>();
        var taskToUri = new Dictionary<Task<string>, string>();

        foreach (var uri in ReplicaAddresses.OrderBy(uri => _replyTimes.GetValueOrDefault(uri, 0)))
        {
            var webRequest = CreateRequest(uri + "?query=" + query);
            
            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            var currentTask = ProcessRequestAsync(webRequest);
            stopwatches.Add(currentTask, Stopwatch.StartNew());
            taskList.Add(currentTask);
            taskToUri.Add(currentTask, uri);

            var timeoutTask = Task.Delay(replicaTimeout);

            _replyTimes.TryAdd(uri, (long)replicaTimeout.TotalMilliseconds);

            while (taskList.Count > 0)
            {
                var resultTask = await Task.WhenAny(taskList.Concat(new[] { timeoutTask }));
                if (taskList.Any(task => task == resultTask))
                {
                    var completedTask = (resultTask as Task<string>)!;
                    stopwatches[completedTask].Stop();
                    try
                    {
                        var result = completedTask.Result;
                        var goodReplicaAddress = taskToUri[completedTask];
                        _replyTimes[goodReplicaAddress] = stopwatches[completedTask].ElapsedMilliseconds;
                        return result;
                    }
                    catch
                    {
                        taskList.Remove(completedTask);

                        var badReplicaAddress = taskToUri[completedTask];
                        _replyTimes[badReplicaAddress] = long.MaxValue;
                            
                        badReplicasTimes.Add(stopwatches[completedTask].ElapsedMilliseconds);
                        replicaTimeout = (timeout - TimeSpan.FromMilliseconds(badReplicasTimes.Sum()))
                                         / (ReplicaAddresses.Length - badReplicasTimes.Count);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(AdvancedSmartClusterClient));
}