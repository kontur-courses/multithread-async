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
            var tasks = ReplicaAddresses.Select(uri =>
            {
                var webRequest = CreateRequest(uri + "?query=" + query);

                Log.InfoFormat($"Processing {webRequest.RequestUri}");

                return ProcessRequestAsync(webRequest);
            }).Concat(new[] { Task.Delay(timeout) });

            var tasksList = tasks.ToList();
            
            while (tasksList.Count > 1) // NOTE: если остался один Task, то это точно timeout, так как иначе мы бы его уже словили
            {
                var taskResult = await Task.WhenAny(tasksList);
                if (taskResult is not Task<string> requestTask)
                    throw new TimeoutException();
            
                try
                {
                    return requestTask.Result;
                }
                catch
                {
                    tasksList.Remove(taskResult);
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
