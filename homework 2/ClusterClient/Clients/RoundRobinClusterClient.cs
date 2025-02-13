using System;
using System.Linq;
using System.Net;
using System.Threading;
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
            var replicas = GetSortedReplicasBySpeed();
            var totalReplicas = replicas.Count;
            var startTime = DateTime.UtcNow;

            foreach (var uri in replicas)
            {
                var elapsedTime = DateTime.UtcNow - startTime;
                var remainingTime = timeout - elapsedTime;
                
                if (remainingTime <= TimeSpan.Zero)
                    break;
                
                var individualTimeout = TimeSpan.FromMilliseconds(remainingTime.TotalMilliseconds / (totalReplicas--));

                try
                {
                    var webRequest = CreateRequest(uri + "?query=" + query);
                    Log.InfoFormat($"Sending request to {webRequest.RequestUri} with timeout {individualTimeout.TotalMilliseconds} ms");

                    using var cts = new CancellationTokenSource(individualTimeout);
                    var task = ProcessRequestAsync(webRequest);

                    var completedTask = await Task.WhenAny(task, Task.Delay(individualTimeout, cts.Token));

                    if (completedTask == task)
                    {
                        var result = await task;
                        if (!string.IsNullOrEmpty(result))
                            return result;
                    }

                    Log.WarnFormat($"Request to {uri} timed out.");
                }
                catch (WebException ex)
                {
                    Log.WarnFormat($"Request to {uri} failed with {ex.Message}");
                }
            }

            throw new TimeoutException($"All replicas failed or timed out within {timeout.TotalMilliseconds} ms.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
