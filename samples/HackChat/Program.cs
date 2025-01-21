using System;
using System.Threading;

namespace HackChat
{
	static class Program
	{
		static void Main(string[] args)
		{
			var chat = new Chat(args.Length > 0 ? int.Parse(args[0]) : Chat.DefaultPort);
			chat.Start();

			Thread.Sleep(-1);

			GC.KeepAlive(chat);
		}
	}
}
