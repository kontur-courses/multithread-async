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
    public class ParallelClusterClient : ClusterClientBase
    {
        public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);

			var tasks = ReplicaAddresses.Select(addr => CreateRequestTaskWithCancel(addr, query, cts.Token)).ToList();

			while (tasks.Count > 0)
			{
				var completedTask = await Task.WhenAny(tasks);

				if (completedTask.IsCompletedSuccessfully)
				{
					cts.Cancel();
					return completedTask.Result;
				}

				tasks.Remove(completedTask);
			}

			throw new TimeoutException();
		}

		protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
