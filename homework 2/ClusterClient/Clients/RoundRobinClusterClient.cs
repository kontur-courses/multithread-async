using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query));

            var replicaCount = ReplicaAddresses.Length;

            foreach (var task in tasks)
            {
                var localTimeout = timeout / replicaCount;
                var currentTask = ProcessRequestAsync(task);
                var delayTask = Task.Delay(localTimeout);

                await Task.WhenAny(currentTask, delayTask);

                if (delayTask.IsCompleted)
                {
                    replicaCount--;
                    continue;
                }

                if (currentTask.Result != null)
                {
                    return currentTask.Result;
                }
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
