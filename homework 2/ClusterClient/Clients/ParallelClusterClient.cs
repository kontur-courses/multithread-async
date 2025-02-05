using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
                .Select(uri => CreateRequest($"{uri}?query={query}"))
                .Select(SilentProcessRequestAsync)
                .ToList();
            var timeoutTask = Task.Delay(timeout);

            while (tasks.Count != 0)
            {
                var processTask = Task.WhenAny(tasks);
                await Task.WhenAny(timeoutTask, processTask);
                
                if (timeoutTask.IsCompleted)
                    throw new TimeoutException();
                if (processTask.Result.Result != null)
                    return processTask.Result.Result;
                
                tasks.Remove(processTask.Result);
            }

            return null;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
