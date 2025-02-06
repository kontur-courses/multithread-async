using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        private (long, string)[] _replicaStatistics;

        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
            _replicaStatistics = Enumerable.Range(0, ReplicaAddresses.Length)
                .Select(i => ((long)0, ReplicaAddresses[i])).ToArray();
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReorderNow()
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .Select((req, i) => (task: TryProcessRequestAsync(req), i));
            var previousTasks = new List<Task<string>>();
            var replicaCount = ReplicaAddresses.Length;

            foreach (var (task, i) in tasks)
            {
                previousTasks.Add(task);
                var localTimeout = timeout / (replicaCount - i);

                var timer = Stopwatch.StartNew();
                var currentTask = Task.WhenAny(previousTasks);
                var delayTask = Task.Delay(localTimeout);

                await Task.WhenAny(currentTask, delayTask);
                timer.Stop();
                timeout -= TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);
                _replicaStatistics[i] =
                    (timer.ElapsedMilliseconds, _replicaStatistics[i].Item2);

                if (currentTask.IsCompleted)
                {
                    if (currentTask.Result.Result is not null) return currentTask.Result.Result;
                    if (currentTask.Result.Result is null) previousTasks.Remove(currentTask.Result);
                }
            }
            throw new TimeoutException();
        }

        private string[] ReorderNow()
        {
            _replicaStatistics = _replicaStatistics.OrderBy(t => t.Item1).ToArray();

            return _replicaStatistics.Select(t => t.Item2).ToArray();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
