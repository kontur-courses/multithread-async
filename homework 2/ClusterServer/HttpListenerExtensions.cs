using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Cluster
{
    public static class HttpListenerExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpListenerExtensions));

        public static void StartProcessingRequestsSync(this HttpListener listener, Action<HttpListenerContext> callbackSync)
        {
            listener.Start();

            while(true)
            {
                try
                {
                    if(!listener.IsListening)
                        return;

                    var context = listener.GetContext();

                    Task.Run(() =>
                    {
                        try
                        {
                            callbackSync(context);
                        }
                        catch(Exception e)
                        {
                            Log.Error(e);
                        }
                        finally
                        {
                            context.Response.Close();
                        }
                    });
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public async static Task StartProcessingRequestsAsync(this HttpListener listener, Func<HttpListenerContext, Task> callbackAsync)
        {
            listener.Start();

            while(true)
            {
                try
                {
                    if(!listener.IsListening)
                        return;

                    var context = await listener.GetContextAsync();
                    Task.Run(
                        async () =>
                        {
                            var ctx = context;
                            try
                            {
                                await callbackAsync(ctx);
                            }
                            catch(Exception e)
                            {
                                Log.Error(e);
                            }
                            finally
                            {
                                ctx.Response.Close();
                            }
                        }
                    );
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}