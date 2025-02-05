using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasksWithIdx = ReplicaAddresses
            .Select((uri, i) =>
            {
                var webRequest = CreateRequest(uri + "?query=" + query);
                return (TryProcessRequestAsync(webRequest), i);
            });

        foreach (var (task, i) in tasksWithIdx)
        {
            var singleTimeout = timeout / (ReplicaAddresses.Length - i);
            var sw = Stopwatch.StartNew();
            await Task.WhenAny(task, Task.Delay(singleTimeout));
            timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
            
            if (task.IsCompleted && task.Result is not null) 
            {
                return task.Result;
            }
        }
        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}