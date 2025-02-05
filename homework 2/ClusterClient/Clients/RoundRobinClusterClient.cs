using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
                .Select(uri => CreateRequest($"{uri}?query={query}"))
                .Select((req, i) => (SilentProcessRequestAsync(req), i));
            
            foreach (var (task, i) in tasks)
            {
                var timeoutTask = Task.Delay(timeout / (ReplicaAddresses.Length - i));
                
                var sw = Stopwatch.StartNew();
                await Task.WhenAny(task, timeoutTask);
                timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                
                if (task.IsCompleted && task.Result != null) 
                    return task.Result;
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
