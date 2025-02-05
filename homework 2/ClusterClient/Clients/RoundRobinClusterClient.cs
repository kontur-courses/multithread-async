using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient : ClusterClientBase
{
    private readonly Dictionary<string, long> _replicaAddressesStatistics;
    private readonly Stopwatch _requestTimer = new();

    public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
        _replicaAddressesStatistics = new Dictionary<string, long>(replicaAddresses.Length);
        foreach (var replicaAddress in replicaAddresses)
            _replicaAddressesStatistics[replicaAddress] = 0;
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var badRequestCount = 0;
        var replicaAddressesByPriority = ReplicaAddresses
            .OrderBy(x => _replicaAddressesStatistics[x])
            .ToArray();

        foreach (var replicaAddress in replicaAddressesByPriority)
        {
            var request = CreateRequest(replicaAddress, query);
            var requestTimeout = timeout / (ReplicaAddresses.Length - badRequestCount);

            Log.InfoFormat($"Processing request {request.RequestUri}");

            var requestTask = ProcessRequestAsync(request);
            var timeoutTask = Task.Delay(requestTimeout);

            _requestTimer.Restart();
            var finishedTask = await Task.WhenAny(requestTask, timeoutTask);
            _requestTimer.Stop();

            _replicaAddressesStatistics[replicaAddress] = _requestTimer.ElapsedMilliseconds;

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