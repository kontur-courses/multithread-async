using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient(string[] replicaAddresses) : ClusterClientWithHistory(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var replicas = OrderedReplicas();
            var tasks = replicas
                .Select(uri => CreateRequest($"{uri}?query={query}"))
                .Select((req, i) => (SilentProcessRequestAsync(req), i));
            var previousTasks = new List<Task<string>>();

            foreach (var (task, i) in tasks)
            {
                previousTasks.Add(task);
                var previousTask = Task.WhenAny(previousTasks);
                var timeoutTask = Task.Delay(timeout / (ReplicaAddresses.Length - i));
                
                var sw = Stopwatch.StartNew();
                await Task.WhenAny(previousTask, timeoutTask); 
                var elapsed = sw.ElapsedMilliseconds;
                
                timeout -= TimeSpan.FromMilliseconds(elapsed);
                ReorderReplicas(replicas[i], elapsed);
                
                if (!previousTask.IsCompleted) 
                    continue;
                
                if (previousTask.Result.Result is not null) 
                    return previousTask.Result.Result;
                if (previousTask.Result.Result is null) 
                    previousTasks.Remove(previousTask.Result);
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
