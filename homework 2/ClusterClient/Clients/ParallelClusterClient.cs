using System;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient : ClusterClientBase
{
    public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var requestTask = await ReplicaAddresses
            .Select(x => CreateRequest(x, query))
            .Select(x =>
            {
                Log.InfoFormat($"Processing request {x.RequestUri}");
                return ProcessRequestAsync(x);
            })
            .WhenAnyCompleteSuccessfully(timeout);

        if (requestTask.IsCompletedSuccessfully)
            return await requestTask;

        throw requestTask.Exception!;
    }

    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
}