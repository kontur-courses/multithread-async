using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using System.Net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
                .Select(address => ProcessRequestWithTimeoutAsync(CreateRequest($"{address}?query={query}"), timeout))
                .ToList();

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                if (completedTask.Status == TaskStatus.RanToCompletion)
                    return await completedTask;
            }

            throw new TimeoutException();
        }

        private async Task<string> ProcessRequestWithTimeoutAsync(WebRequest webRequest, TimeSpan timeout)
        {
            var requestTask = ProcessRequestAsync(webRequest);
            var delayTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(requestTask, delayTask);

            if (completedTask == delayTask)
                throw new TimeoutException();

            return await requestTask;
        }
        
        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
