using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Clients.Priority
{
	public class Priority
	{
		public int NotSuccess = 0;
		public TimeSpan SuccesRequestsTime = TimeSpan.Zero;

		public bool IsLessThan(Priority other)
		{
			if (NotSuccess == other.NotSuccess)
				return SuccesRequestsTime < other.SuccesRequestsTime;

			return NotSuccess < other.NotSuccess;
		}
	}
}
