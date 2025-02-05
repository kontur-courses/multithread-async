using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var replicaTimeout = timeout / ReplicaAddresses.Length;
            var taskList = new List<Task<string>>();
            var badReplicasTimes = new List<long>();
            var stopwatches = new Dictionary<Task<string>, Stopwatch>();

            foreach (var uri in ReplicaAddresses)
            {
                var webRequest = CreateRequest(uri + "?query=" + query);
            
                Log.InfoFormat($"Processing {webRequest.RequestUri}");

                var currentTask = ProcessRequestAsync(webRequest);
                stopwatches.Add(currentTask, Stopwatch.StartNew());
                taskList.Add(currentTask);

                var timeoutTask = Task.Delay(replicaTimeout);

                while (taskList.Count > 0)
                {
                    var resultTask = await Task.WhenAny(taskList.Concat(new[] { timeoutTask }));
                    if (taskList.Any(task => task == resultTask))
                    {
                        var completedTask = (resultTask as Task<string>)!;
                        stopwatches[completedTask].Stop();
                        try
                        {
                            return completedTask.Result;
                        }
                        catch
                        {
                            taskList.Remove(completedTask);
                            
                            badReplicasTimes.Add(stopwatches[completedTask].ElapsedMilliseconds);
                            replicaTimeout = (timeout - TimeSpan.FromMilliseconds(badReplicasTimes.Sum()))
                                             / (ReplicaAddresses.Length - badReplicasTimes.Count);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
