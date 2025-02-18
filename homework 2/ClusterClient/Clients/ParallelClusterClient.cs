using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
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
            var tasks = new List<Task>();
            foreach (var uri in ReplicaAddresses)
            {
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                tasks.Add(ProcessRequestAsync(webRequest));
            }
            
            var timeoutTask = Task.Delay(timeout);

            while (!tasks.IsNullOrEmpty())
            {
                var completedTask = await Task.WhenAny(tasks.Append(timeoutTask));

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException();
                }

                if (completedTask.IsCompletedSuccessfully)
                {
                    return await (Task<string>)completedTask;
                }

                tasks.Remove(completedTask);
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
