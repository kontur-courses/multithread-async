using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses.Select(async replica =>
            {
                var fullUri = CombineQueryUri(replica, query);
                var request = CreateRequest(fullUri);
                return await ProcessRequestAsync(request, timeout);
            }).ToList();

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                if (completedTask.IsCompletedSuccessfully)
                {
                    return completedTask.Result;
                }

                tasks.Remove(completedTask);
            }

            throw new TimeoutException("All requests timed out or bad.");
        }


        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}