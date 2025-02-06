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
    public class ParallelClusterClient : ClusterClientBase
    {
        public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var tasks = ReplicaAddresses
                .Select(x => CreateRequest(x + "?query=" + query))
                .Select(x => ProcessRequestAsync(x).ExecuteWithTimeoutAsync(cts.Token))
                .ToList();

            while (tasks.Count != 0)
            {
                try
                {
                    var task = await Task.WhenAny(tasks);
                    tasks.Remove(task);
                    var result = await task;
                    await cts.CancelAsync();

                    return result;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to process the request {query}. Error: {ex.Message}.", ex);
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
