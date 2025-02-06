using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Models;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient : ClusterClientBase
{
    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));

    private readonly Instance[] _instances;

    public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
        _instances = ReplicaAddresses.Select(address => new Instance(0, address)).ToArray();
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasks = _instances
            .OrderBy(s => s.Time)
            .Select(s => CreateRequest(s.Address + "?query=" + query))
            .Select((req, i) => (task: TryProcessRequestAsync(req), i));

        foreach (var (task, i) in tasks)
        {
            var sw = Stopwatch.StartNew();
            await Task.WhenAny(task, Task.Delay(timeout / (ReplicaAddresses.Length - i)));
            sw.Stop();
            timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
            _instances[i] = _instances[i] with { Time = sw.ElapsedMilliseconds };
            if (task.IsCompleted && task.Result is not null) return task.Result;
        }

        throw new TimeoutException();
    }
}