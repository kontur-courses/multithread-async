using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientWithHistory(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var replicas = OrderedReplicas();
            var tasksWithPos = replicas
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .Select((req, i) => (task: SilentProcessRequestAsync(req), i));
            
            Console.WriteLine("Current order is: " + replicas
                .Select(r => Array.IndexOf(replicaAddresses, r))
                .Aggregate(string.Empty, (s, i) => $"{s}, {i}")
            );

            foreach (var (task, i) in tasksWithPos)
            {
                var singleTimeout = timeout / (replicaAddresses.Length - i);
                
                var sw = Stopwatch.StartNew();
                await Task.WhenAny(task, Task.Delay(singleTimeout)); 
                
                var elapsed = sw.ElapsedMilliseconds;
                timeout -= TimeSpan.FromMilliseconds(elapsed);
                ReorderReplicas(elapsed, replicas[i]);
                
                if (task.IsCompleted && task.Result is not null) return task.Result;
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
