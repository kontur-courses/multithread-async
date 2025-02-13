using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses) { }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var startTime = DateTime.UtcNow;
            var activeTasks = new List<Task<string>>();
            var remainingReplicas = ReplicaAddresses.Length;

            foreach (var uri in GetSortedReplicasBySpeed())
            {
                var elapsed = DateTime.UtcNow - startTime;
                var remainingTime = timeout - elapsed;
                if (remainingTime <= TimeSpan.Zero || remainingReplicas <= 0)
                    break;

                var perReplicaTimeout = remainingTime / remainingReplicas;
                var webRequest = CreateRequest($"{uri}?query={query}");
                Log.InfoFormat($"Sending request to {webRequest.RequestUri}");

                try
                {
                    var requestTask = ProcessRequestAsync(webRequest);
                    activeTasks.Add(requestTask);

                    var completedTask = await Task.WhenAny(activeTasks.Append(Task.Delay(perReplicaTimeout)));

                    if (completedTask is Task<string> resultTask)
                    {
                        if (resultTask.IsCompletedSuccessfully)
                        {
                            var result = await resultTask;
                            if (!string.IsNullOrEmpty(result))
                                return result;
                        }
                        else
                        {
                            activeTasks.Remove(resultTask);
                        }
                    }
                    else
                    {
                        remainingReplicas--;
                    }
                }
                catch (WebException ex)
                {
                    Log.WarnFormat($"Request to {uri} failed with {ex.Message}");
                    remainingReplicas--;
                }
            }

            if (activeTasks.Count > 0)
            {
                var finalRemainingTime = timeout - (DateTime.UtcNow - startTime);
                if (finalRemainingTime > TimeSpan.Zero)
                {
                    var completedTask = await Task.WhenAny(activeTasks.Append(Task.Delay(finalRemainingTime)));
                    if (completedTask is Task<string> { IsCompletedSuccessfully: true } finalResultTask)
                        return await finalResultTask;
                }
            }

            throw new TimeoutException($"All requests failed or timed out after {timeout.TotalMilliseconds} ms.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
