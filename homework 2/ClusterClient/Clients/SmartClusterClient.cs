using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient : ClusterClientBase
{
    private readonly List<ReplicaTime> _replicaStatistics;
    
    public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
        _replicaStatistics = ReplicaAddresses
            .Select(address => new ReplicaTime(0, address))
            .ToList();
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var replicaCount = ReplicaAddresses.Length;
        var requests = _replicaStatistics
            .OrderBy(r => r.Time)
            .Select(r => CreateRequest($"{r.Address}?query={query}"))
            .ToList();
        var requestTasks = new List<Task<string>>();
        for (var i = 0; i < replicaCount; i++)
        {
            var request = requests[i];
            var perRequestTimeout = timeout / (replicaCount - i);
            
            var sw = Stopwatch.StartNew();
            var timeoutTask = Task.Delay(perRequestTimeout);
            var requestTask = ProcessRequestAsync(request);
            Log.InfoFormat($"Processing request {request.RequestUri}");
            requestTasks.Add(requestTask);
            var completedTask = await Task.WhenAny(requestTasks.Append(timeoutTask));
            sw.Stop();
            
            var workTimeInMilliseconds = sw.ElapsedMilliseconds;
            timeout -= TimeSpan.FromMilliseconds(workTimeInMilliseconds);
            _replicaStatistics[i].Time = workTimeInMilliseconds;
            if (timeoutTask == completedTask)
                continue;

            if (completedTask.IsCompletedSuccessfully)
                return await (Task<string>)completedTask;
            requestTasks.Remove((Task<string>)completedTask);
        }
        throw new TimeoutException();
    }
    
    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}