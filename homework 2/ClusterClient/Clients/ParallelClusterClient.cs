using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasks = ReplicaAddresses
            .Select(uri => CreateRequest($"{uri}?query={query}"))
            .Select(TryProcessRequestAsync)
            .ToList();

        var delayTask = Task.Delay(timeout);

        while (tasks.Count != 0)
        {
            var currentTask = Task.WhenAny(tasks);

            await Task.WhenAny(currentTask, delayTask);

            if (delayTask.IsCompleted)
            {
                throw new TimeoutException();
            }

            if (currentTask.Result.Result != null)
            {
                return currentTask.Result.Result;
            }

            tasks.Remove(currentTask.Result);
        }

        return null;
    }

    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
}