using ClusterClient.Clients.Priority;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ClusterClient.Clients
{
	public class PriorityRoundRobinClusterClient(string[] replicaAddresses) : PriorityClusterClientBase(replicaAddresses)
	{
		public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
		{
			var timer = new Stopwatch();
			var aliveReplicasCount = ReplicaAddresses.Length;

			var excludedReplicas = new HashSet<ReplicaItem>();

			while(excludedReplicas.Count < ReplicaAddresses.Length)
			{
				var cts = new CancellationTokenSource();
				var replica = GetFastestReplica(excludedReplicas);
				excludedReplicas.Add(replica);

				timer.Restart();

				var timeoutTask = Task.Delay(timeout / aliveReplicasCount, cts.Token);
				var requestTask = CreateRequestTaskWithCancel(replica.Adress, query, cts.Token);

				await Task.WhenAny(requestTask, timeoutTask);
				timer.Stop();
				cts.Cancel();

				if (requestTask.IsCompletedSuccessfully)
					return requestTask.Result;
				else if (requestTask.IsFaulted)
				{
					timeout = timeout.Subtract(timer.Elapsed);
					aliveReplicasCount--;
				}

				var request = new Request(timer.Elapsed, requestTask.IsCompletedSuccessfully);
				UpdateReplicas(replica, request);
			}

			throw new TimeoutException();
		}

		protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
	}
}
