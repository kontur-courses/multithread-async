using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HackChat
{
	public class Chat
	{
		public const int DefaultPort = 31337;

		private readonly byte[] PingMsg = new byte[1];
		private readonly ConcurrentDictionary<IPEndPoint, (TcpClient Client, NetworkStream Stream)> Connections = new();

		private readonly int port;
		private readonly TcpListener tcpListener;

		public Chat(int port) => tcpListener = new TcpListener(IPAddress.Any, this.port = port);

		public void Start()
		{
			// "Поток" обнаружения клиентов
			Task.Run(DiscoverLoop);
			// "Поток" пользовательского ввода сообщения и отправки клиентам
			Task.Run(() =>
			{
				string line;
				while((line = Console.ReadLine()) != null)
					Task.Run(() => BroadcastAsync(line));
			});
			// Поток обработки входящих подключений
			Task.Run(() =>
			{
				tcpListener.Start(100500);
				while(true)
				{
					var tcpClient = tcpListener.AcceptTcpClient();
					Task.Run(() => ProcessClientAsync(tcpClient));
				}
			});
		}

		private async Task BroadcastAsync(string message)
		{
			// Необходимо реализовать метод, который будет отправлять сообщение message 
			// Всем подключенным клиентам (к которым подключились мы и которые подключены к нам)
			// Отправку тоже можно распараллелить используя Parallel.ForEachAsync
			throw new NotImplementedException();
		}

		private async void DiscoverLoop()
		{
			while(true)
			{
				try { await Discover(); } catch { /* ignored */ }
				await Task.Delay(3000);
			}
		}

		private async Task Discover()
		{
			// В данном методе необходимо реализовать механизм
			// * Проверки подключен ли клиент к нам или мы к нему
			// * Поиска клиентов (обход ip адресов и портов)
			// * Проверка доступности (поможет предыдущий эксперимент NMAP)
			// * Подключение к по tcp
			// * Запуск обработки сообщений в отдельном потоке
			throw new NotImplementedException();
		}

		private static async Task ProcessClientAsync(TcpClient tcpClient)
		{
			EndPoint endpoint = null;
			try { endpoint = tcpClient.Client.RemoteEndPoint; } catch { /* ignored */ }
			await Console.Out.WriteLineAsync($"[{endpoint}] connected");
			try
			{
				using(tcpClient)
				{
					var stream = tcpClient.GetStream();
					await ReadLinesToConsoleAsync(stream);
				}
			}
			catch { /* ignored */ }
			await Console.Out.WriteLineAsync($"[{endpoint}] disconnected");
		}

		private static async Task ReadLinesToConsoleAsync(Stream stream)
		{
			string line;
			using var sr = new StreamReader(stream);
			while((line = await sr.ReadLineAsync()) != null)
				await Console.Out.WriteLineAsync($"[{((NetworkStream)stream).Socket.RemoteEndPoint}] {line}");
		}

		/// <summary>
		///     Метод получения свободного порта
		/// </summary>
		private static int GetFreePort()
		{
			// Запускаем TcpListener на 0 порту, тогда система сама выберет нам доступный порт
			var listener = new TcpListener(IPAddress.Loopback, 0);
			try
			{
				listener.Start(); // Запускаем, в этот момент нам выдается порт
				return ((IPEndPoint)listener.LocalEndpoint).Port; // Получаем порт и отдаем его
			}
			finally
			{
				listener.Stop(); // Как только мы вышли из метода (return) выполняется блок finally и останавливается слушатель с освобождением порта
			}
		}
	}
}