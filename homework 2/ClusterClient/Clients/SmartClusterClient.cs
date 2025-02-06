using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBaseWithHistory(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasksWithIdx = OrderedReplicas()
            .Select((uri, i) =>
            {
                var webRequest = CreateRequest(uri + "?query=" + query);
                return (TryProcessRequestAsync(webRequest), i);
            });
        var prevTasks = new List<Task<string>>();

        foreach (var (task, i) in tasksWithIdx)
        {
            prevTasks.Add(task);
            var prevTask = Task.WhenAny(prevTasks);
            var singleTimeout = timeout / (ReplicaAddresses.Length - i);
            
            var sw = Stopwatch.StartNew();
            await Task.WhenAny(prevTask, Task.Delay(singleTimeout));
            timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);

            if (!prevTask.IsCompleted)
            {
                continue;
            }

            if (prevTask.Result.Result is not null)
            {
                return prevTask.Result.Result;
            }
            prevTasks.Remove(prevTask.Result);
        }
        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}