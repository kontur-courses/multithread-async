using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public async override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .Select(ProcessRequestAsync).ToHashSet();
            var timer = Task.Delay(timeout);
            var tasksAll = tasks.Append(timer);

            Task res;
            do
            {
                res = await Task.WhenAny(tasksAll);
                tasks.Remove(res as Task<string>);
            }
            while (res.IsFaulted && tasks.Count > 0);

            if (res == timer)
                throw new TimeoutException();

           
            return await (res as Task<string>);
            
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
