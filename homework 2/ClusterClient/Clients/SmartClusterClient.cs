using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient : ClusterClientBase
{
    public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var startTime = DateTime.Now;
        var activeTasks = new List<Task<string>>();

        for (var i = 0; i < ReplicaAddresses.Length; i++)
        {
            var elapsed = DateTime.Now - startTime;
            var remainingTime = timeout - elapsed;
            if (remainingTime <= TimeSpan.Zero)
                break;

            var remainingReplicas = ReplicaAddresses.Length - i;
            var perReplicaTimeout = remainingTime / remainingReplicas;

            var uri = ReplicaAddresses[i];
            var webRequest = CreateRequest(uri + "?query=" + query);
            Log.InfoFormat($"Sending request to {webRequest.RequestUri}");

            try
            {
                var requestTask = ProcessRequestAsync(webRequest);
                activeTasks.Add(requestTask);

                var timeoutTask = Task.Delay(perReplicaTimeout);
                var completedTask = await Task.WhenAny(activeTasks.Append(timeoutTask));

                if (completedTask == timeoutTask)
                    continue;

                var resultTask = (Task<string>)completedTask;
                if (resultTask.IsCompletedSuccessfully)
                {
                    var result = await resultTask;
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }

                activeTasks.Remove(resultTask);
            }
            catch (WebException ex)
            {
                Log.WarnFormat($"Request to {uri} failed with {ex.Message}");
            }
        }

        var finalRemainingTime = timeout - (DateTime.Now - startTime);
        if (finalRemainingTime > TimeSpan.Zero)
        {
            var finalTimeoutTask = Task.Delay(finalRemainingTime);
            var completedTask = await Task.WhenAny(activeTasks.Append(finalTimeoutTask));

            if (completedTask is Task<string> { IsCompletedSuccessfully: true } finalResultTask)
                return await finalResultTask;
        }

        throw new TimeoutException($"All requests failed or timed out after {timeout.TotalMilliseconds} ms.");
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}