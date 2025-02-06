using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient : ClusterClientBase
{
    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));

    public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var tasks = ReplicaAddresses
            .Select(baseUri => CreateRequest(baseUri + "?query=" + query))
            .Select(TryProcessRequestAsync)
            .ToList();
        var delay = Task.Delay(timeout);

        while (tasks.Count != 0)
        {
            var task = Task.WhenAny(tasks);

            await Task.WhenAny(task, delay);

            if (delay.IsCompleted)
            {
                throw new TimeoutException();
            }

            if (task.Result.Result != null)
            {
                return task.Result.Result;
            }

            tasks.Remove(task.Result);
        }

        return null;
    }
}