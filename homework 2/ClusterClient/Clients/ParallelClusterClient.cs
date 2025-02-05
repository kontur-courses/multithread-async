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
            var ctx = new CancellationTokenSource(timeout);
            
            var tasks = ReplicaAddresses.Select(async uri =>
            {
                var webRequest = CreateRequest($"{uri}?query={query}");
                
                Log.InfoFormat($"Processing {uri}");

                await using var registration = ctx.Token.Register(webRequest.Abort);
                return await ProcessRequestAsync(webRequest);
            }).ToHashSet();
            
            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                if (!completedTask.IsCompletedSuccessfully) continue;
                await ctx.CancelAsync();
                return await completedTask;
            }

            var timeoutException = new TimeoutException();
            
            Log.Error("Timeout", timeoutException);

            throw timeoutException;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
