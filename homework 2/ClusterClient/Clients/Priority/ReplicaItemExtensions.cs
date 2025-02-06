using System;

namespace ClusterClient.Clients.Priority
{
	static class ReplicaItemExtensions
	{
		public static void ConcatNeighbors(this ReplicaItem replica)
		{
			if (replica.Previous != null)
				replica.Previous.Next = replica.Next;

			if (replica.Next != null)
				replica.Next.Previous = replica.Previous;
		}

		public static bool TryFindNextItem(this ReplicaItem replica, out ReplicaItem target, Func<ReplicaItem, bool> isFind)
		{
			var current = replica;
			while (current != null)
			{
				if (isFind(current))
					break; 
				current = current.Next;
			}

			target = current;
			return target != null;
		}
	}
}
