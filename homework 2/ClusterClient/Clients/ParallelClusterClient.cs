using System;
using System.Linq;
using System.Net;
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
            var tasks = ReplicaAddresses.Select(async uri =>
            {
                try
                {
                    var webRequest = CreateRequest(uri + "?query=" + query);
                    Log.InfoFormat($"Sending request to {webRequest.RequestUri}");
                    return await ProcessRequestAsync(webRequest);
                }
                catch (WebException ex)
                {
                    Log.WarnFormat($"Request to {uri} failed with {ex.Message}");
                    return null;
                }
            }).ToList();

            var timeoutTask = Task.Delay(timeout);

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks.Append(timeoutTask));

                if (completedTask == timeoutTask)
                    throw new TimeoutException("No successful response received from any replica.");

                tasks = tasks.Where(t => t != completedTask).ToList();

                var result = await (Task<string>)completedTask;
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            throw new TimeoutException($"All requests failed or timed out after {timeout.TotalMilliseconds} ms.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
