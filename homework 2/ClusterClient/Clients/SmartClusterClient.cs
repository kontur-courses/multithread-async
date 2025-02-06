using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
			var aliveReplicasCount = ReplicaAddresses.Length;
			var cts = new CancellationTokenSource();
			var timer = new Stopwatch();
			var tasks = new List<Task>();

			foreach(var addr in ReplicaAddresses)
			{
				timer.Restart();

				var timeoutTask = Task.Delay(timeout / aliveReplicasCount, cts.Token);
				var requestTask = CreateRequestTaskWithCancel(addr, query, cts.Token);
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

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
