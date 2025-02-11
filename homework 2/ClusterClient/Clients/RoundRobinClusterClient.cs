using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasks = ReplicaAddresses
            .Select((uri, index) =>
            {
                var request = CreateRequest($"{uri}?query={query}");
                return (TryProcessRequestAsync(request), index);
            });

        foreach (var (task, index) in tasks)
        {
            var singleTimeout = timeout / (ReplicaAddresses.Length - index);
            var timer = Stopwatch.StartNew();

            await Task.WhenAny(task, Task.Delay(singleTimeout));

            timer.Stop();
            timeout -= TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);

            if (task.IsCompleted && task.Result is not null)
            {
                return task.Result;
            }
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
}