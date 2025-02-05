using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

		public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
		{
			var timer = new Stopwatch();
			var aliveReplicasCount = ReplicaAddresses.Length;

			foreach(var addr in ReplicaAddresses)
			{
				var cts = new CancellationTokenSource();

				timer.Restart();

				var timeoutTask = Task.Delay(timeout / aliveReplicasCount, cts.Token);
				var requestTask = CreateRequestTaskWithCancel(addr, query, cts.Token);

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
			}

			throw new TimeoutException();
		}

		protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
