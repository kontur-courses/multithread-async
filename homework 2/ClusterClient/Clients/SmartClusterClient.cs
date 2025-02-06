using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
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
            var listTasks = new List<Task<string>>();
            using var cts = new CancellationTokenSource(timeout);

            var smartTimeout = timeout / ReplicaAddresses.Length;
            

            foreach (var replicaAddress in ReplicaAddresses)
            {
                var webRequest = CreateRequest(replicaAddress + "?query=" + query);
                var delayTask = Task.Delay(smartTimeout, cts.Token);

                listTasks.Add(ProcessRequestAsync(webRequest).WaitAsync(cts.Token));
                var task = await Task.WhenAny(listTasks.Append(delayTask));

                if (task is Task<string> requestTask)
                {
                    try
                    {
                        var result = await requestTask;
                        await cts.CancelAsync();

                        return result;
                    }
                    catch (WebException ex)
                    {
                        listTasks.Remove(requestTask);
                        Log.ErrorFormat($"Request [{webRequest.Address}] failed: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat($"Request [{webRequest.Address}] failed: {ex.Message}");
                    }
                }
            }

            return await WaitForCompletingRestTime(listTasks);
        }

        private async Task<string> WaitForCompletingRestTime(List<Task<string>> listTasks)
        {
            while (listTasks.Count != 0)
            {
                var completedTask = await Task.WhenAny(listTasks);
                listTasks.Remove(completedTask);

                try
                {
                    return await completedTask;
                }
                catch (WebException ex)
                {
                    Log.Error($"Request are bad, error: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error while processing request TaskId = {completedTask.Id}, error: {ex.Message}.", ex);
                }
            }

            throw new TimeoutException("All tasks failed or timeout.");
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
