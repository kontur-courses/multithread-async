using System;
using System.Linq;
using System.Threading;
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
            using var cts = new CancellationTokenSource(timeout);
            var tasks = ReplicaAddresses
                .Select(addr => CreateRequest($"{addr}?query={query}"))
                .Select(request => ProcessRequestAsync(request).WaitAsync(cts.Token)).ToHashSet();

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);

                if (completedTask.IsCompletedSuccessfully)
                {
                    await cts.CancelAsync();
                    return completedTask.Result;
                }
                
                if (completedTask.IsFaulted)
                {
                    Log.Error($"Task {completedTask.Id} failed with error: {completedTask.Exception}");
                    tasks.Remove(completedTask);
                }
                
                if (completedTask.IsCanceled)
                {
                    Log.Error($"Task {completedTask.Id} failed with cancellation token: {completedTask.Exception}");
                    tasks.Remove(completedTask);
                }
            }
            
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
