using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientWithHistory(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var replicas = OrderedReplicas();
            var tasks = replicas
                .Select(uri => CreateRequest($"{uri}?query={query}"))
                .Select((req, i) => (SilentProcessRequestAsync(req), i));
            
            foreach (var (task, i) in tasks)
            {
                var timeoutTask = Task.Delay(timeout / (ReplicaAddresses.Length - i));
                
                var sw = Stopwatch.StartNew();
                await Task.WhenAny(task, timeoutTask); 
                var elapsed = sw.ElapsedMilliseconds;
                
                timeout -= TimeSpan.FromMilliseconds(elapsed);
                ReorderReplicas(replicas[i], elapsed);
                
                if (task.IsCompleted && task.Result != null) 
                    return task.Result;
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
