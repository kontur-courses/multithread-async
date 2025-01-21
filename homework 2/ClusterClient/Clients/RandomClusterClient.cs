using System;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RandomClusterClient : ClusterClientBase
    {
        private readonly Random random = new Random();

        public RandomClusterClient(string[] replicaAddresses)
            : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var uri = ReplicaAddresses[random.Next(ReplicaAddresses.Length)];

            var webRequest = CreateRequest(uri + "?query=" + query);
            
            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            var resultTask = ProcessRequestAsync(webRequest);

            await Task.WhenAny(resultTask, Task.Delay(timeout));
            if (!resultTask.IsCompleted)
                throw new TimeoutException();

            return resultTask.Result;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
    }
}