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
            var count = ReplicaAddresses.Length;
            foreach (var uri in ReplicaAddresses)
            {
                var specialTimeout = timeout / count--;
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                var resultTask = ProcessRequestAsync(webRequest);
                var stopWatch = Stopwatch.StartNew();
                var completedTask = await Task.WhenAny(resultTask, Task.Delay(specialTimeout));
                timeout -= TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
                if (completedTask == resultTask && completedTask.IsCompletedSuccessfully)
                    return resultTask.Result;
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
