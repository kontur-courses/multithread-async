using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient : ClusterClientBase
{
    public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var remainingTime = timeout;
        var startTime = DateTime.Now;

        foreach (var uri in ReplicaAddresses)
        {
            var perReplicaTimeout = TimeSpan.FromMilliseconds(
                remainingTime.TotalMilliseconds / 
                (ReplicaAddresses.Length - Array.IndexOf(ReplicaAddresses, uri)));

            try
            {
                var request = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat($"Sending request to {request.RequestUri}");

                var requestTask = ProcessRequestAsync(request);
                var completedTask = await Task.WhenAny(requestTask, Task.Delay(perReplicaTimeout));

                if (completedTask == requestTask)
                    return await requestTask;
            }
            catch (WebException ex)
            {
                Log.WarnFormat($"Request to {uri} failed with {ex.Message}");
            }

            remainingTime = timeout - (DateTime.Now - startTime);
            if (remainingTime <= TimeSpan.Zero)
                break;
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}