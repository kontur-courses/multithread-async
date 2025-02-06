using System;
using System.IO;
using System.Reflection;
using System.Text;
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

            try
            {
                if(!ServerOptions.TryGetArguments(args, out var parsedArguments))
                    return;

                var server = new ClusterServer(parsedArguments, Log);
                server.Start();

                Log.InfoFormat("Press ENTER to stop listening");
                Console.ReadLine();
                Log.InfoFormat("Server stopped!");
            }
            catch(Exception e)
            {
                Log.Fatal(e);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
    }
}