using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var devidedTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / ReplicaAddresses.Length);
        var badRequestsCount = 0;
        var tasks = ReplicaAddresses
            .Select(uri => CreateRequest(uri + "?query=" + query))
            .Select(request =>
            {
                Log.InfoFormat($"Processing {request.RequestUri}");
                return ProcessRequestAsync(request);
            });

        foreach (var task in tasks)
        {
            using var cts = new CancellationTokenSource(devidedTimeout);
            try
            {
                var result = await task.RunTaskWithCancellation(cts.Token);
                if (task.IsCompletedSuccessfully)
                    return result;
            }
            catch (WebException e)
            {
                badRequestsCount++;
                devidedTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / (ReplicaAddresses.Length - badRequestsCount));
            }
            catch
            {
                // ignored
            }
        }
        
        throw new TimeoutException("Task timed out");
        
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}