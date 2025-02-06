using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();
            var replicasCount = ReplicaAddresses.Length;
            var tasks = new List<Task>();

            foreach (var uri in ReplicaAddresses)
            {
                var request = CreateRequest($"{uri}?query={query}");
                var taskTimeout = (timeout - stopwatch.Elapsed) / replicasCount;
                replicasCount--;

                var resultTask = ProcessRequestAsync(request);
                var delay = Task.Delay(taskTimeout);

                tasks.Add(resultTask);
                tasks.Add(delay);

                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(delay);

                if (completedTask is Task<string> completedResultTask)
                {
                    if (completedResultTask.IsCompletedSuccessfully)
                    {
                        return completedResultTask.Result;
                    }

                    tasks.Remove(completedResultTask);
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
