using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Common;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var timeoutPerReplica = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / ReplicaAddresses.Length);
            var runningTasks = new List<Task<string>>();

            foreach (var replicaAddress in ReplicaAddresses)
            {
                var request = CreateRequest(replicaAddress + "?query=" + query);
                runningTasks.Add(ProcessRequestAsync(request).RunTaskWithTimeoutAsync(cts.Token));

                var delayTask = Task.Delay(timeoutPerReplica, cts.Token);
                var completedTask = await Task.WhenAny(runningTasks.Append(delayTask));
                if (completedTask is Task<string> successfulTask)
                {
                    try
                    {
                        var result = await successfulTask;
                        await cts.CancelAsync();

                        return result;
                    }
                    catch (WebException ex)
                    {
                        runningTasks.Remove(successfulTask);
                        Log.Error($"Request {request} are bad, error: {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error while processing request {request}, error: {ex.Message}.", ex);
                    }
                }
            }

            while (runningTasks.Count != 0)
            {
                var completedTask = await Task.WhenAny(runningTasks);
                runningTasks.Remove(completedTask);

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