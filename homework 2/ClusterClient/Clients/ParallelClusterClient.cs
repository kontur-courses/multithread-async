using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            using var cts = new CancellationTokenSource(timeout);

            var taskList = ReplicaAddresses.Select(replicaAddress =>
            {
                var webRequest = CreateRequest(replicaAddress + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                return ProcessRequestAsync(webRequest).WaitAsync(cts.Token);
            }).ToList();

            while (taskList.Count > 0)
            {
                var completedTask = await Task.WhenAny(taskList);

                if (completedTask.IsCompletedSuccessfully)
                {
                    cts.Cancel();
                    return completedTask.Result;
                }
                else if (completedTask.IsFaulted)
                {
                    Log.ErrorFormat($"Task {completedTask.Id} failed: {completedTask.Exception}");
                    taskList.Remove(completedTask);
                }
                else if (completedTask.IsCanceled)
                {
                    Log.ErrorFormat($"Task {completedTask.Id} canceled");
                    taskList.Remove(completedTask);
                }
            }

            throw new TimeoutException("All tasks failed or timeout.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
