using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Clients.Priority
{
	public readonly struct Request(TimeSpan time, bool isSuccess)
	{
		public bool IsSuccess => isSuccess;
		public TimeSpan Time => time;
	}
}
