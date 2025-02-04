using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasksWithPos = replicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .Select((req, i) => (task: SilentProcessRequestAsync(req), i));
            var previousTasks = new List<Task<string>>();

            foreach (var (task, i) in tasksWithPos)
            {
                previousTasks.Add(task);
                var previousTask = Task.WhenAny(previousTasks);
                var singleTimeout = timeout / (replicaAddresses.Length - i);
                
                var sw = Stopwatch.StartNew();
                await Task.WhenAny(previousTask, Task.Delay(singleTimeout));
                timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);

                if (previousTask.IsCompleted)
                {
                    if (previousTask.Result.Result is not null) return previousTask.Result.Result;
                    if (previousTask.Result.Result is null) previousTasks.Remove(previousTask.Result);
                }
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
