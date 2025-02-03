using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Common;
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
            var timeoutPerReplica = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / ReplicaAddresses.Length);
            var badRequestCount = 0;
            foreach (var replicaAddress in ReplicaAddresses)
            {
                using var cts = new CancellationTokenSource(timeoutPerReplica);
                var request = CreateRequest(replicaAddress + "?query=" + query);
                var processRequest = ProcessRequestAsync(request);

                try
                {
                    return await processRequest.RunTaskWithTimeoutAsync(cts.Token);
                }
                catch (WebException ex)
                {
                    badRequestCount++;
                    timeoutPerReplica = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / (ReplicaAddresses.Length - badRequestCount));
                    Log.Error($"Request {request} are bad, error: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error while processing request {query}, error: {ex.Message}.", ex);
                }
            }

            throw new TimeoutException("All requests timed out or bad.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}