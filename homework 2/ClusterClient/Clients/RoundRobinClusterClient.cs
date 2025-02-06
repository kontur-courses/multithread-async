using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
            var timeoutForReplica = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / ReplicaAddresses.Length);
            var badRequestCount = 0;
            foreach (var replicaAddress in ReplicaAddresses)
            {
                using var cts = new CancellationTokenSource(timeoutForReplica);
                var request = CreateRequest(replicaAddress + "?query=" + query);
                var processRequest = ProcessRequestAsync(request);

                try
                {
                    return await processRequest.ExecuteWithTimeoutAsync(cts.Token);
                }
                catch (WebException ex)
                {
                    badRequestCount++;
                    timeoutForReplica = TimeSpan
                        .FromMilliseconds(timeout.TotalMilliseconds / (ReplicaAddresses.Length - badRequestCount));
                    Log.Error($"Bad request {request}: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to process the request {query}. Error: {ex.Message}.", ex);
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
