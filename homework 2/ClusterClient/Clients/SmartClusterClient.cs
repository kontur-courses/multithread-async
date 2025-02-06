using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using ClusterClient.Models;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));

        private readonly Instance[] _instances;

        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
            _instances = ReplicaAddresses.Select(address => new Instance(0, address)).ToArray();
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = _instances
                .OrderBy(s => s.Time)
                .Select(s => CreateRequest(s.Address + "?query=" + query))
                .Select((req, i) => (task: TryProcessRequestAsync(req), i));
            var previousTasks = new List<Task<string>>();

            foreach (var (t, i) in tasks)
            {
                previousTasks.Add(t);
                var sw = Stopwatch.StartNew();
                var task = Task.WhenAny(previousTasks);
                var delay = Task.Delay(timeout / (ReplicaAddresses.Length - i));
                await Task.WhenAny(task, delay);
                sw.Stop();
                timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                _instances[i] = _instances[i] with {Time = sw.ElapsedMilliseconds};
                if (task.IsCompleted)
                {
                    if (task.Result.Result is not null) return task.Result.Result;
                    previousTasks.Remove(task.Result);
                };
            }

            throw new TimeoutException();
        }
    }
}