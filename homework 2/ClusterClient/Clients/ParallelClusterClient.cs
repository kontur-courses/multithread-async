using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses.Select(async uri =>
            {
                var webRequest = CreateRequest($"{uri}?query={query}");
                
                Log.InfoFormat($"Processing {uri}");
                
                return await ProcessRequestAsync(webRequest);
            }).ToHashSet();
            
            while (tasks.Count > 0)
            {
                var delayTask = Task.Delay(timeout);
                var completedTask = await Task.WhenAny(tasks.Append(delayTask));
                
                if (completedTask == delayTask) 
                    break;
                
                if (completedTask.IsCompletedSuccessfully)
                    return await (Task<string>)completedTask;
                
                if (completedTask is Task<string> resultTask)
                    tasks.Remove(resultTask);
            }

            var timeoutException = new TimeoutException();
            
            Log.Error("Timeout", timeoutException);

            throw timeoutException;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
