using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
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
                timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);

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
