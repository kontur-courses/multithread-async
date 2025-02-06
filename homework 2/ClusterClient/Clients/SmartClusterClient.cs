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
            var countOfReplicas = ReplicaAddresses.Length;
            var workingTasks = new List<Task<string>>();
            foreach (var uri in ReplicaAddresses)
            {
                var perReplicaTimeout = timeout / countOfReplicas--;
                var webRequest = CreateRequest(uri + "?query=" + query);
                var task = ProcessRequestAsync(webRequest);
                workingTasks.Add(task);

                var sw = Stopwatch.StartNew();
                var completedTask = await Task.WhenAny(workingTasks.Append(Task.Delay(perReplicaTimeout)));
                timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);

                if (completedTask is not Task<string> resultTask) continue;
                if (resultTask.IsCompletedSuccessfully && resultTask.Result is not null)
                {
                    return resultTask.Result;
                }

                workingTasks.Remove(resultTask);
            }

            throw new TimeoutException("Request to the replicas timed out.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}