using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var count = ReplicaAddresses.Length;
            var tasks = new List<Task<string>>();
            foreach (var uri in ReplicaAddresses)
            {
                var specialTimeout = timeout / count--;
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                var resultTask = ProcessRequestAsync(webRequest);
                tasks.Add(resultTask);
                var stopWatch = Stopwatch.StartNew();
                var completedTask = await Task.WhenAny(tasks.Append(Task.Delay(specialTimeout)));
                timeout -= TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
                if (completedTask is not Task<string>)
                    continue;
                if (completedTask.IsCompletedSuccessfully)
                    return await (Task<string>)completedTask;

                tasks.Remove(resultTask);
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
