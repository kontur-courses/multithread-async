using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
                .Select(async uri =>
                {
                    var webRequest = CreateRequest(uri + "?query=" + query);
                    try
                    {
                        return await ProcessRequestAsync(webRequest);
                    }
                    catch (WebException ex)
                    {
                        Log.WarnFormat("{0}: {1}", webRequest.RequestUri, ex.Message);
                    }

                    return null;
                })
                .ToList();

            while (tasks.Count != 0)
            {
                var processTask = Task.WhenAny(tasks);
                var delayTask = Task.Delay(timeout);
                await Task.WhenAny(processTask, delayTask);
                if (delayTask.IsCompleted)
                {
                    throw new TimeoutException();
                }

                if (processTask.Result.Result is not null)
                {
                    return processTask.Result.Result;
                }
                tasks.Remove(processTask.Result);
            }
            return null;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
