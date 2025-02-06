using ClusterClient.Clients.Priority;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ClusterClient.Clients
{
	public abstract class PriorityClusterClientBase : ClusterClientBase
	{
		protected ReplicaItem Root;

		protected ReplicaItem Tail;

		protected const int HistoryLength = 100;

		private readonly ReaderWriterLockSlim rwLock = new();

		public PriorityClusterClientBase(string[] replicaAddresses)
			: base(replicaAddresses)
		{
			if (replicaAddresses == null || replicaAddresses.Length == 0)
				throw new ArgumentNullException(nameof(replicaAddresses));

			Root = new ReplicaItem(replicaAddresses.First(), HistoryLength);

			var current = Root;
			for (int i = 1; i < replicaAddresses.Length; i++)
			{
				var next = new ReplicaItem(replicaAddresses[i], HistoryLength)
				{
					Previous = current
				};
				current.Next = next;
				current = next;
			}

			Tail = current;
		}

		protected ReplicaItem GetFastestReplica(HashSet<ReplicaItem> excluded)
		{
			rwLock.EnterReadLock();
			try
			{
				Root.TryFindNextItem(out var target, item => !excluded.Contains(item));
				return target ?? Root;
			}
			finally { rwLock.ExitReadLock(); }
		}


		protected void UpdateReplicas(ReplicaItem replica, Request request)
		{
			rwLock.EnterWriteLock();
			try
			{
				replica.PushRequest(request);

				replica.ConcatNeighbors();

				InsertReplica(replica);
			}
			finally { rwLock.ExitWriteLock(); }
		}

		protected void InsertReplica(ReplicaItem replica)
		{
			if (Root.TryFindNextItem(out var current, item => !item.Priority.IsLessThan(replica.Priority)))
			{
				replica.Next = current;
				replica.Previous = current.Previous;

				if (current.Previous == null)
					Root = replica;
				else
					current.Previous.Next = replica;

				current.Previous = replica;
				return;
			}

			Tail.Next = replica;
			replica.Previous = Tail;
			Tail = replica;
			replica.Next = null;
		}
	}
}
