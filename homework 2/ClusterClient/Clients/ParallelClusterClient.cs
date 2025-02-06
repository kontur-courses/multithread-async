using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient : ClusterClientBase
    {
        public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var delay = Task.Delay(timeout);
            var tasks = ReplicaAddresses
                .Select(uri => ProcessRequestAsync(CreateRequest($"{uri}?query={query}")))
                .Append(delay)
                .ToList();

            while (tasks.Count > 1)
            {
                var completedTask = await Task.WhenAny(tasks);

                if (completedTask == delay)
                {
                    break;
                }

                if (completedTask.IsCompletedSuccessfully)
                {
                    return ((Task<string>)completedTask).Result;
                }

                tasks.Remove(completedTask);
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
