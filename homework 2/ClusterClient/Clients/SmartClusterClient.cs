using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var completedTaskCount = 0;
            var tasks = new HashSet<Task<string>>();

            foreach (var request in ReplicaAddresses.Select(address => CreateRequest($"{address}?query={query}")))
            {
                var currentTimeout = timeout / (ReplicaAddresses.Length - completedTaskCount);

                Log.InfoFormat($"Processing {request.RequestUri}");

                var task = ProcessRequestAsync(request);
                tasks.Add(task);
                var delay = Task.Delay(currentTimeout);

                var completedTask = await Task.WhenAny(tasks.Append(delay));

                if (completedTask == delay)
                    continue;

                if (completedTask.IsCompletedSuccessfully)
                    return await (Task<string>)completedTask;

                if (completedTask is Task<string> resultTask)
                    tasks.Remove(resultTask);

                completedTaskCount++;
            }

            var timeoutException = new TimeoutException();

            Log.Error("Timeout", timeoutException);

            throw timeoutException;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}