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
    public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var countOfReplicas = ReplicaAddresses.Length;
            foreach (var uri in ReplicaAddresses)
            {
                var perReplicaTimeout = timeout / countOfReplicas--;
                var webRequest = CreateRequest(uri + "?query=" + query);
                var task = ProcessRequestAsync(webRequest);
                var sw = Stopwatch.StartNew();
                var completedTask = await Task.WhenAny(task, Task.Delay(perReplicaTimeout));
                timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);

                if (completedTask == task && task.IsCompletedSuccessfully && !string.IsNullOrEmpty(task.Result))
                {
                    return task.Result;
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}