using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HackChat
{
    public class Chat
    {
        public const int DefaultPort = 27010;

        private readonly byte[] PingMsg = new byte[1];
        private readonly ConcurrentDictionary<IPEndPoint, (TcpClient Client, NetworkStream Stream)> Connections = new();

        private readonly int port;
        private readonly TcpListener tcpListener;

        public Chat(int port) => tcpListener = new TcpListener(IPAddress.Any, this.port = port);

        public void Start()
        {
            Task.Run(DiscoverLoop);
            Task.Run(() =>
            {
                string line;
                while ((line = Console.ReadLine()) != null)
                    Task.Run(() => BroadcastAsync(line));
            });
            Task.Run(() =>
            {
                tcpListener.Start(100500);
                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();
                    Task.Run(() => ProcessClientAsync(tcpClient));
                }
            });
        }

        private async Task BroadcastAsync(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            foreach (var connection in Connections)
            {
                await connection.Value.Stream.WriteAsync(bytes);
            }
        }

        private async void DiscoverLoop()
        {
            while (true)
            {
                try
                {
                    await Discover();
                }
                catch
                {
                    /* ignored */
                }

                await Task.Delay(3000);
            }
        }

        private async Task Discover()
        {
            await Parallel.ForAsync(27000, 27040, async (port, token) =>
            {
                var x = IPAddress.Parse("0.0.0.0");
                var tcpClient = new TcpClient(new IPEndPoint(x, DefaultPort));

                var connetionTask = await tcpClient.ConnectAsync(IPAddress.Parse("127.0.0.1"), port, 1000);
                if (connetionTask.Status is not TaskStatus.RanToCompletion)
                {
                    tcpClient.Dispose();
                    return;
                }

                if (Connections.Keys.Any(x => x.Address.ToString().Split(":").Last() == port.ToString()))
                {
                    tcpClient.Dispose();
                    return;
                }

                Task.Run(() => ProcessClientAsync(tcpClient));
            });
        }

        private async Task ProcessClientAsync(TcpClient tcpClient)
        {
            IPEndPoint endpoint = null;
            try
            {
                endpoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
            }
            catch
            {
                /* ignored */
            }

            await Console.Out.WriteLineAsync($"[{endpoint}] connected");
            try
            {
                using (tcpClient)
                {
                    var stream = tcpClient.GetStream();
                    Connections.TryAdd(endpoint, (tcpClient, stream));
                    await ReadLinesToConsoleAsync(stream);
                }
            }
            catch
            {
                /* ignored */
            }

            await Console.Out.WriteLineAsync($"[{endpoint}] disconnected");
        }

        private static async Task ReadLinesToConsoleAsync(Stream stream)
        {
            string line;
            using var sr = new StreamReader(stream);
            while ((line = await sr.ReadLineAsync()) != null)
                await Console.Out.WriteLineAsync($"[{((NetworkStream)stream).Socket.RemoteEndPoint}] {line}");
        }
    }
}