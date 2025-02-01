using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses)
        : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var webRequests = new HttpWebRequest[ReplicaAddresses.Length];
            for (var i = 0; i < webRequests.Length; i++)
            {
                webRequests[i] = CreateRequest($"{ReplicaAddresses[i]}?query={query}");
            }

            var tasks = new List<Task>(webRequests.Length + 1);
            for (var i = 0; i < webRequests.Length; i++)
            {
                Log.InfoFormat($"Processing {webRequests[i].RequestUri}");
                tasks.Add(ProcessRequestAsync(webRequests[i]));
            }

            var timeoutTask = Task.Delay(timeout);
            tasks.Add(timeoutTask);

            while (tasks.Count > 1)
            {
                var completedTask = await Task.WhenAny(tasks);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException();
                }

                if (completedTask.IsCompletedSuccessfully)
                {
                    return ((Task<string>)completedTask).Result;
                }

                tasks.Remove(completedTask);
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
