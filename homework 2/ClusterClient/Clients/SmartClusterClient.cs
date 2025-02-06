using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var sw = new Stopwatch();
            var addressesCount = ReplicaAddresses.Length;
            var tasks = new HashSet<Task>();
            
            sw.Start();
            foreach (var address in ReplicaAddresses)
            {
                var request = CreateRequest($"{address}?query={query}");
                var timeForRequest = (timeout - sw.Elapsed) / addressesCount;
                var task = ProcessRequestAsync(request).WaitAsync(cts.Token);
                tasks.Add(task);
                tasks.Add(Task.Delay(timeForRequest, cts.Token));
                var completedTask = await Task.WhenAny(tasks);

                if (completedTask.IsCompletedSuccessfully && completedTask is Task<string> resultTask)
                {
                    await cts.CancelAsync();
                    return resultTask.Result;
                }
                
                tasks.Remove(completedTask);
                addressesCount--;
            }
        
            return await WaitCompletedTaskOrThrow(tasks, cts);
        }

        private async Task<string> WaitCompletedTaskOrThrow(HashSet<Task> tasks, CancellationTokenSource cts)
        {
            while (tasks.Count > 0)
            {
                var task = await Task.WhenAny(tasks);
                tasks.Remove(task);

                if (task is not Task<string> { IsCompletedSuccessfully: true } resultTask) 
                    continue;
                await cts.CancelAsync();
                return resultTask.Result;
            }
            
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
