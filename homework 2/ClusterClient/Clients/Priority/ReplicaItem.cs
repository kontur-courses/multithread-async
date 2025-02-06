using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Clients.Priority
{
	public class ReplicaItem(string address, int maxHistoryLength)
	{
		private readonly Priority priority = new();

		private readonly Queue<Request> lastRequests = new();

		public ReplicaItem Next { get; set; }
		public ReplicaItem Previous { get; set; }

		public string Adress => address;

		public Priority Priority => priority;

		public void PushRequest(Request request)
		{
			if (!request.IsSuccess)
			{
				priority.NotSuccess++;
				return;
			}

			priority.SuccesRequestsTime += request.Time;

			if (lastRequests.Count == maxHistoryLength)
				priority.SuccesRequestsTime -= lastRequests.Dequeue().Time;

			lastRequests.Enqueue(request);
		}
	}
}
