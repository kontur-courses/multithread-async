using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasksWithIdx = ReplicaAddresses
            .Select((uri, index) =>
            {
                var request = CreateRequest($"{uri}?query={query}");
                return (TryProcessRequestAsync(request), index);
            });
        var previousTasks = new List<Task<string>>();

        foreach (var (task, index) in tasksWithIdx)
        {
            previousTasks.Add(task);
            var previousTask = Task.WhenAny(previousTasks);
            var singleTimeout = timeout / (ReplicaAddresses.Length - index);
            var timer = Stopwatch.StartNew();

            await Task.WhenAny(previousTask, Task.Delay(singleTimeout));

            timer.Stop();
            timeout -= TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);

            if (!previousTask.IsCompleted)
            {
                continue;
            }

            if (previousTask.Result.Result is not null)
            {
                return previousTask.Result.Result;
            }

            previousTasks.Remove(previousTask.Result);
        }

        throw new TimeoutException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}