using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Common;
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
            using var globalCts = new CancellationTokenSource(timeout);
            var tasks = ReplicaAddresses
                .Select(x => CreateRequest(x + "?query=" + query))
                .Select(x => ProcessRequestAsync(x).RunTaskWithTimeoutAsync(globalCts.Token))
                .ToList();

            while (tasks.Count != 0)
            {
                try
                {
                    var task = await Task.WhenAny(tasks);
                    tasks.Remove(task);
                    var stringResult = await task;
                    await globalCts.CancelAsync();

                    return stringResult;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error while processing request {query}, error: {ex.Message}.", ex);
                }
            }

            throw new TimeoutException("All requests timed out or bad.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}