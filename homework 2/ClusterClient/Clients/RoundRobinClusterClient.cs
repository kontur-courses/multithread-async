using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        private (long, string)[] _replicaStatistics;

        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
            _replicaStatistics = Enumerable.Range(0, ReplicaAddresses.Length)
                .Select(i => ((long)0, ReplicaAddresses[i])).ToArray();
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReorderNow()
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .Select((req, i) => (task: TryProcessRequestAsync(req), i)); ;

            var replicaCount = ReplicaAddresses.Length;


            foreach (var (task, i) in tasks)
            {
                var localTimeout = timeout / (replicaCount - i);

                var timer = Stopwatch.StartNew();

                var delayTask = Task.Delay(localTimeout);

                await Task.WhenAny(task, delayTask);
                timer.Stop();
                timeout -= TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);
                _replicaStatistics[i] =
                    (timer.ElapsedMilliseconds, _replicaStatistics[i].Item2);

                if (task.IsCompleted && task.Result is not null) return task.Result;
            }
            throw new TimeoutException();
        }

        private string[] ReorderNow()
        {
            _replicaStatistics = _replicaStatistics.OrderBy(t => t.Item1).ToArray();

            return _replicaStatistics.Select(t => t.Item2).ToArray();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
