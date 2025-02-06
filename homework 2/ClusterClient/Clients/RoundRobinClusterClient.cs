using System;
using System.Collections.Generic;
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
            var notRespondedReplica = ReplicaAddresses.Length;
            foreach (var replica in ReplicaAddresses)
            {
                var timeoutPerReplica = timeout / notRespondedReplica;
                var resultUri = CombineQueryUri(replica, query);
                var request = CreateRequest(resultUri);
                var processRequest = ProcessRequestAsync(request,timeoutPerReplica);
                try
                {
                    return await processRequest;
                }
                catch (WebException ex)
                {
                    Log.Error($"Request {request} are bad, error: {ex.Message}", ex);
                    notRespondedReplica--;
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