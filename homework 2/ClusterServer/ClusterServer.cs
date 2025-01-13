using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Cluster
{
	public class ClusterServer
    {
        public ClusterServer(ServerOptions serverOptions, ILog log)
        {
            this.ServerOptions = serverOptions;
            this.log = log;
        }

        public void Start()
        {
            if(Interlocked.CompareExchange(ref isRunning, Running, NotRunning) == NotRunning)
            {
                httpListener = new HttpListener
                {
                    Prefixes =
                    {
                        $"http://127.0.0.1:{ServerOptions.Port}/{ServerOptions.MethodName}/"
                    }
                };

                log.InfoFormat($"Server is starting listening prefixes: {string.Join(";", httpListener.Prefixes)}");
                if(ServerOptions.Async)
                    httpListener.StartProcessingRequestsAsync(CreateAsyncCallback(ServerOptions.MethodDuration, ServerOptions.Status));
                else
                    httpListener.StartProcessingRequestsSync(CreateSyncCallback(ServerOptions.MethodDuration, ServerOptions.Status));
            }
        }

        public void Stop()
        {
            if(Interlocked.CompareExchange(ref isRunning, NotRunning, Running) == Running)
            {
                if(httpListener.IsListening)
                    httpListener.Stop();
            }
        }

        public ServerOptions ServerOptions { get; }

        private Action<HttpListenerContext> CreateSyncCallback(int methodDuration, int status = 200)
        {
            return context =>
            {
                var currentRequestId = Interlocked.Increment(ref RequestsCount);
                var query = context.Request.QueryString["query"];
                log.InfoFormat($"Thread #{Thread.CurrentThread.ManagedThreadId} received request '{query}' #{currentRequestId} at {DateTime.Now.TimeOfDay}");

                Thread.Sleep(methodDuration);

                context.Response.StatusCode = status;
                var encryptedBytes = ClusterHelpers.GetBase64HashBytes(query);
                context.Response.OutputStream.Write(encryptedBytes, 0, encryptedBytes.Length);

                log.InfoFormat($"Thread #{query} sent response {status} for '{Thread.CurrentThread.ManagedThreadId}' for #{currentRequestId} at {DateTime.Now.TimeOfDay}");
            };
        }

        private Func<HttpListenerContext, Task> CreateAsyncCallback(int methodDuration, int status = 200)
        {
            return async context =>
            {
                var currentRequestNum = Interlocked.Increment(ref RequestsCount);
                var query = context.Request.QueryString["query"];
                log.InfoFormat($"Thread #{Thread.CurrentThread.ManagedThreadId} received request '{query}' #{currentRequestNum} at {DateTime.Now.TimeOfDay}");

                await Task.Delay(methodDuration);
                // Thread.Sleep(methodDuration);

                context.Response.StatusCode = status;
                var encryptedBytes = ClusterHelpers.GetBase64HashBytes(query);
                await context.Response.OutputStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);

                log.InfoFormat($"Thread #{Thread.CurrentThread.ManagedThreadId} sent response {status} for '{query}' #{currentRequestNum} at {DateTime.Now.TimeOfDay}");
            };
        }

        private int RequestsCount;

        private int isRunning = NotRunning;

        private const int Running = 1;
        private const int NotRunning = 0;

        private readonly ILog log;
        private HttpListener httpListener;
    }
}
