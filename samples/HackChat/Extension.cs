using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HackChat
{
	public static class Extensions
	{
		public static async Task<Task> ConnectAsync(this TcpClient tcpClient, IPAddress ipAddr, int port, int timeout = 3000)
		{
			var connectTask = tcpClient.ConnectAsync(ipAddr, port);
			await Task.WhenAny(connectTask, Task.Delay(timeout));
			return connectTask;
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach(var item in enumerable)
				action(item);
		}
	}

	public static class ConsoleHelper
	{
		public static void ColoredWrite(string msg, ConsoleColor color)
		{
			lock(Console.Out)
			{
				Console.ForegroundColor = color;
				Console.Out.WriteLine(msg);
				Console.ResetColor();
			}
		}
	}
}