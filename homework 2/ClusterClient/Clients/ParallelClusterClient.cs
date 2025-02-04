using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Clients.Extensions;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses)
        : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            using var cancellation = new CancellationTokenSource(timeout);
            Log.InfoFormat($"Start processing query \"{query}\" with {timeout} timeout");

            var tasks = new List<Task<string>>(ReplicaAddresses.Length);
            foreach (var address in ReplicaAddresses)
            {
                var request = CreateRequest($"{address}?query={query}");
                Log.InfoFormat($"Processing {request.RequestUri}");

                var requestTask = Task.Run(async () =>
                {
                    cancellation.Token.Register(request.Abort);
                    return await ProcessRequestAsync(request);
                }, cancellation.Token);

                tasks.Add(requestTask);
            }

            var timeoutMessage = $"No positive response received for query \"{query}\" with {timeout} timeout";
            return await tasks.WaitForFirstSuccessAsync(timeoutMessage, cancellation);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
