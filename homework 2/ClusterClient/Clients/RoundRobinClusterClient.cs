using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var completedTaskCount = 0;

            foreach (var replicaAddress in ReplicaAddresses)
            {
                var currentTimeout = timeout / (ReplicaAddresses.Length - completedTaskCount);
                
                var request = CreateRequest($"{replicaAddress}?query={query}");
                
                Log.InfoFormat($"Processing {replicaAddress}");
                
                var task = ProcessRequestAsync(request);
                var delay = Task.Delay(currentTimeout);

                var completedTask  = await Task.WhenAny(task, delay);

                if (completedTask == delay)
                    continue;

                if (completedTask.IsCompletedSuccessfully)
                    return await (Task<string>)completedTask;
                
                completedTaskCount++;
            }

            var timeoutException = new TimeoutException();
            
            Log.Error("Timeout", timeoutException);

            throw timeoutException;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}