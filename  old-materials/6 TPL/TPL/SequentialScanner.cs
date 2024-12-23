using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TPL
{
	public class SequentialScanner : IPScanner
	{
		public Task Scan(IPAddress[] ipAddrs, int[] ports)
		{
			foreach(var ipAddr in ipAddrs)
			{
				if(PingAddr(ipAddr) != IPStatus.Success)
					continue;

				foreach(var port in ports)
					CheckPort(ipAddr, port);
			}

			return Task.CompletedTask;
		}

		private IPStatus PingAddr(IPAddress ipAddr, int timeout = 3000)
		{
			using var ping = new Ping();

			Console.WriteLine($"Pinging {ipAddr}");
			var status = ping.Send(ipAddr, timeout).Status;
			Console.WriteLine($"Pinged {ipAddr}: {status}");
			
			return status;
		}

		private static void CheckPort(IPAddress ipAddr, int port, int timeout = 3000)
		{
			using var tcpClient = new TcpClient();
			
			Console.WriteLine($"Checking {ipAddr}:{port}");
			var portStatus = tcpClient.Connect(ipAddr, port, timeout); 
			Console.WriteLine($"Checked {ipAddr}:{port} - {portStatus}");
		}
	}
}