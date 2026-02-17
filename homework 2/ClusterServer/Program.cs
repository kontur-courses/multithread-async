using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using log4net.Config;

namespace Cluster
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), new FileInfo("log4net.config"));
            ThreadPool.SetMinThreads(15, 100);
            ThreadPool.SetMaxThreads(30, 1000);
            Console.WriteLine($"PID: {Process.GetCurrentProcess().Id}");

            try
            {
                if(!ServerOptions.TryGetArguments(args, out var parsedArguments))
                    return;

                var server = new ClusterServer(parsedArguments, log);
                server.Start();

                log.InfoFormat("Press ENTER to stop listening");
                Console.ReadLine();
                log.InfoFormat("Server stopped!");
            }
            catch(Exception e)
            {
                log.Fatal(e);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
    }
}