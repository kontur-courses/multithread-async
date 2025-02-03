using System;
using System.Linq;
using System.Net;
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
            var tasks = ReplicaAddresses
                .Select(x => CreateRequest(x + "?query=" + query))
                .Select(ProcessRequestAsync)
                .ToList();

            while (tasks.Count != 0)
            {
                Task<string> completedTask = null;
                try
                {
                    var delayTask = Task.Delay(timeout);
                    var task  = await Task.WhenAny(tasks.Concat([delayTask]));

                    if (task is not Task<string> stringTask)
                        throw new TimeoutException();

                    completedTask = stringTask;

                    return await stringTask;
                }
                catch (WebException)
                {
                    tasks.Remove(completedTask);
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}