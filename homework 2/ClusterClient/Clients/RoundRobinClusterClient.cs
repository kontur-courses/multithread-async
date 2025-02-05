using System;
using System.Linq;
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
        var badRequestCount = 0;

        foreach (var request in ReplicaAddresses.Select(x => CreateRequest(x, query)))
        {
            var requestTimeout = timeout / (ReplicaAddresses.Length - badRequestCount);

            Log.InfoFormat($"Processing request {request.RequestUri}");

            var requestTask = ProcessRequestAsync(request);
            var timeoutTask = Task.Delay(requestTimeout);
            var finishedTask = await Task.WhenAny(requestTask, timeoutTask);

            if (finishedTask == timeoutTask)
                continue;

            if (requestTask.IsCompletedSuccessfully)
                return await requestTask;

            badRequestCount++;
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}