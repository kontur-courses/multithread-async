using ClusterClient.Clients.Priority;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Clients
{
	public class PrioritySmartClusterClient(string[] replicaAddresses) : PriorityClusterClientBase(replicaAddresses)
	{
		public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
		{
			var cts = new CancellationTokenSource();
			var timer = new Stopwatch();
			var aliveReplicasCount = ReplicaAddresses.Length;

			var tasks = new List<Task>();

			var excludedReplicas = new HashSet<ReplicaItem>();

			while(excludedReplicas.Count < ReplicaAddresses.Length)
			{
				var replica = GetFastestReplica(excludedReplicas);
				excludedReplicas.Add(replica);

				timer.Restart();

				var timeoutTask = Task.Delay(timeout / aliveReplicasCount, cts.Token);
				var requestTask = CreateRequestTask(replica, query, cts.Token);
				tasks.Add(requestTask);

				var completedTask = await Task.WhenAny(tasks.Concat([timeoutTask]));
				timer.Stop();

				if (completedTask is Task<string> successTask && successTask.IsCompletedSuccessfully)
				{
					cts.Cancel();
					return successTask.Result;
				}
				else if (completedTask.IsFaulted)
				{
					timeout = timeout.Subtract(timer.Elapsed);
					aliveReplicasCount--;
					tasks.Remove(completedTask);
				}
			}
			cts.Cancel();

			throw new TimeoutException();
		}

		private async Task<string> CreateRequestTask(ReplicaItem replica, string query, CancellationToken token)
		{
			var timer = Stopwatch.StartNew();
			var resultTask = CreateRequestTaskWithCancel(replica.Adress, query, token);
			var result = await resultTask;
			timer.Stop();

			var request = new Request(timer.Elapsed, resultTask.IsCompletedSuccessfully);
			UpdateReplicas(replica, request);

			return await Task.FromResult(result);
		}

		protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
	}
}
