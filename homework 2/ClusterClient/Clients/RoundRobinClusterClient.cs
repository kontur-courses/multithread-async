using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var goodCount = ReplicaAddresses.Length;

            foreach (var replicaAddress in ReplicaAddresses)
            {
                var webRequest = CreateRequest(replicaAddress + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");

                try
                {
                    return await ProcessRequestAsync(webRequest).WaitAsync(timeout / goodCount);
                }
                catch (WebException ex)
                {
                    goodCount--;
                    Log.ErrorFormat($"Request [{webRequest.Address}] failed: {ex.Message}");
                }
                catch(Exception ex)
                {
                    Log.ErrorFormat($"Request [{webRequest.Address}] failed: {ex.Message}");
                }
            }

            throw new TimeoutException("All tasks failed or timeout.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
