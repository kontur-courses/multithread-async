using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses.Select(uri =>
            {
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                return ProcessRequestAsync(webRequest);
            }).ToList();

            var timeoutTask = Task.Delay(timeout);

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks.Append(timeoutTask));

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Request to the replicas timed out.");
                }

                if (completedTask.IsCompletedSuccessfully)
                {
                    return await (Task<string>) completedTask;
                }

                tasks.Remove((Task<string>) completedTask);
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}