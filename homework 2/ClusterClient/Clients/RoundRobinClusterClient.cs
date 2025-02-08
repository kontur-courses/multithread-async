using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        private readonly List<ReplicaTime> _replicaStatistics;
        
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
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
            for (var i = 0; i < replicaCount; i++)
            {
                var request = requests[i];
                var perRequestTimeout = timeout / (replicaCount - i);
                
                var sw = Stopwatch.StartNew();
                var timeoutTask = Task.Delay(perRequestTimeout);
                var requestTask = ProcessRequestAsync(request);
                Log.InfoFormat($"Processing {request.RequestUri}");
                var completedTask = await Task.WhenAny(requestTask, timeoutTask);
                sw.Stop();
                
                var workTimeInMilliseconds = sw.ElapsedMilliseconds;
                timeout -= TimeSpan.FromMilliseconds(workTimeInMilliseconds);
                _replicaStatistics[i].Time = workTimeInMilliseconds;
                if (timeoutTask == completedTask)
                    continue;
                
                if (requestTask.IsCompletedSuccessfully)
                    return await requestTask;
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
