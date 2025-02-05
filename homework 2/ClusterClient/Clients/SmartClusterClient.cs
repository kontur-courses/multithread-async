using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var tasks = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query));
            var previousTasks = new List<Task<string>>();
            var replicaCount = ReplicaAddresses.Length;

            foreach (var task in tasks)
            {
                var localTimeout = timeout / replicaCount;
                var currentTask = ProcessRequestAsync(task);
                var delayTask = Task.Delay(localTimeout);

                previousTasks.Add(currentTask);

                var doneTask = Task.WhenAny(previousTasks);

                await Task.WhenAny(doneTask, delayTask);

                if (delayTask.IsCompleted)
                {
                    replicaCount--;
                    previousTasks.Remove(currentTask);
                    continue;
                }

                if (doneTask.Result.Result != null)
                {
                    return doneTask.Result.Result;
                }
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
