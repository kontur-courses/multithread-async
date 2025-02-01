using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                .Select(x => CreateRequest(x + "?query=" + query))
                .Select(ProcessRequestAsync)
                .Select(process => ProcessAsync(process, cts.Token)).ToList();

            var completedTasks = new List<Task<string>>();
            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                try
                {
                    var result = await completedTask;
                    if (!IsServerError(result))
                    {
                        await cts.CancelAsync();
                        return result;
                    }
                    completedTasks.Add(completedTask);
                }
                catch (Exception ex)
                {
                    Log.Error("Error processing request", ex);
                }
            }

            if (completedTasks.Count > 0)
                return await completedTasks.First();

            throw new TimeoutException("All requests failed or timed out.");
        }

        private static bool IsServerError(string result) => result.Contains("500");

        private static async Task<string> ProcessAsync(Task<string> task, CancellationToken token)
        {
            var delayTask = Task.Delay(-1, token);
            var firstTask = await Task.WhenAny(task, delayTask);

            if (firstTask == delayTask)
                    throw new TimeoutException();

            return await task;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}