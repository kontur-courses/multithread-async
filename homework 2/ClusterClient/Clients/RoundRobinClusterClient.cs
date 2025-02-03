using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient(string[] replicaAddresses)
        : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var workingReplicasAdressesCount = ReplicaAddresses.Length;

            var webRequests = new HttpWebRequest[ReplicaAddresses.Length];
            for (var i = 0; i < webRequests.Length; i++)
            {
                webRequests[i] = CreateRequest($"{ReplicaAddresses[i]}?query={query}");
            }

            for (var i = 0; i < webRequests.Length; i++)
            {
                Log.InfoFormat($"Processing {webRequests[i].RequestUri}");
                var timeoutPerTask = timeout / workingReplicasAdressesCount;
                var timeoutTask = Task.Delay(timeoutPerTask);
                var requestTask = ProcessRequestAsync(webRequests[i]);

                var resultTask = await Task.WhenAny(timeoutTask, requestTask);

                if (resultTask == timeoutTask)
                {
                    continue;
                }

                if (resultTask.IsFaulted)
                {
                    workingReplicasAdressesCount -= 1;
                    if (workingReplicasAdressesCount <= 0)
                    {
                        throw new WebException();
                    }
                    continue;
                }

                if (resultTask.IsCompletedSuccessfully)
                {
                    return ((Task<string>)resultTask).Result;
                }
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
