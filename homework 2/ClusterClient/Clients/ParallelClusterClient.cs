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
            var webRequests = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query));
            var timeoutTask = Task.Delay(timeout);
            
            var tasks = webRequests
                .Select(async r =>
                {
                    Log.InfoFormat($"Processing {r.RequestUri}");
                    return await ProcessRequestAsync(r);
                })
                .Append(timeoutTask)
                .ToList();

            while (tasks.Count > 1)
            {
                var completedTask = await Task.WhenAny(tasks);
                
                if (completedTask == timeoutTask)
                    break;

                if (completedTask.IsCompletedSuccessfully)
                    return await (Task<string>)completedTask;
                
                tasks.Remove(completedTask);
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
