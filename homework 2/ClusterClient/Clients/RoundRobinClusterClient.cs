using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var clietTimeout = timeout / ReplicaAddresses.Length;
            var timeoutForOne = clietTimeout;
            var workClient = ReplicaAddresses.Length;
            foreach (var address in ReplicaAddresses)
            {
                var task = Task.Run(() => ProcessRequestAsync(CreateRequest(address + $"?query={query}")));
                await Task.WhenAny(task, Task.Delay(clietTimeout));

                if (task.Status is TaskStatus.RanToCompletion) return task.Result;
                if (task.Status is TaskStatus.Faulted) clietTimeout += timeoutForOne / --workClient;
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}