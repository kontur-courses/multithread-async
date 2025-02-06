using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = new List<Task<string>>();
            var notRespondedReplica = ReplicaAddresses.Length;
            var timeoutPerReplica = timeout / notRespondedReplica;
            foreach (var replica in ReplicaAddresses)
            {
                var resultUri = CombineQueryUri(replica, query);
                var request = CreateRequest(resultUri);
                var processRequest = ProcessRequestAsync(request, timeout);
                tasks.Add(processRequest);
                var lastCompletedTask = await Task.WhenAny(tasks.Append(Task.Delay(timeoutPerReplica)));
                if (lastCompletedTask is not Task<string> completedTask) continue;
                try
                {
                    return await completedTask;
                }
                catch (WebException ex)
                {
                    Log.Error($"Request {request} are bad, error: {ex.Message}", ex);
                    tasks.Remove(completedTask);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error while processing request {query}, error: {ex.Message}.", ex);
                }
            }

            while (tasks.Count != 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                try
                {
                    return await completedTask;
                }
                catch (WebException ex)
                {
                    Log.Error($"Request are bad, error: {ex.Message}", ex);
                }
            }

            throw new TimeoutException("All requests timed out or bad.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}