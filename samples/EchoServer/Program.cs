using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessTcpServer();
            // AsyncProcessTcpServer();
        }

        private static void ProcessTcpServer()
        {
            var tcpListener = new TcpListener(IPAddress.Any, 31337);
            tcpListener.Start(100500);

            while(true)
            {
                var client = tcpListener.AcceptTcpClient();
                Task.Run(() => ProcessClient(client));
            }
        }

        private static void ProcessClient(TcpClient client)
        {
            try
            {
                using(client)
                {
                    var networkStream = client.GetStream();

                    using(var sr = new StreamReader(networkStream))
                    using(var sw = new StreamWriter(networkStream) {AutoFlush = true})
                    {
                        string line;
                        while((line = sr.ReadLine()) != null)
                        {
                            sw.WriteLine(line);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void AsyncProcessTcpServer()
        {
            var tcpListener = new TcpListener(IPAddress.Any, 31337);
            tcpListener.Start(100500);

            while(true)
            {
                var tcpClient = tcpListener.AcceptTcpClient();
                ProcessClientAsync(tcpClient);
            }
        }

        private static async Task ProcessClientAsync(TcpClient tcpClient)
        {
            try
            {
                using(tcpClient)
                {
                    var stream = tcpClient.GetStream();
                    using(var sr = new StreamReader(stream))
                    using(var sw = new StreamWriter(stream) {AutoFlush = true})
                    {
                        string line;
                        while((line = await sr.ReadLineAsync()) != null)
                            await sw.WriteLineAsync(line);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}