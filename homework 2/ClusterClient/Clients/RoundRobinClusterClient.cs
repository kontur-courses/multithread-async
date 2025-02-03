using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var replicaTimeout = timeout / ReplicaAddresses.Length;
            var badReplicasTimes = new List<long>();
            var sw = new Stopwatch();

            foreach (var uri in ReplicaAddresses)
            {
                var webRequest = CreateRequest(uri + "?query=" + query);
            
                Log.InfoFormat($"Processing {webRequest.RequestUri}");

                sw.Restart();
                var resultTask = ProcessRequestAsync(webRequest);
                var completed = await Task.WhenAny(resultTask, Task.Delay(replicaTimeout));
                sw.Stop();
                
                if (completed == resultTask)
                {
                    try
                    {
                        return resultTask.Result;
                    }
                    catch
                    {
                        badReplicasTimes.Add(sw.ElapsedMilliseconds);
                        replicaTimeout = (timeout - TimeSpan.FromMilliseconds(badReplicasTimes.Sum()))
                                         / (ReplicaAddresses.Length - badReplicasTimes.Count);
                    }
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
