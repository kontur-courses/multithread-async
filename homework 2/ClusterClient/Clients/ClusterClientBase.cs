using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public abstract class ClusterClientBase(string[] replicaAddresses)
    {
        protected string[] ReplicaAddresses { get; set; } = replicaAddresses;

        public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout);
        protected abstract ILog Log { get; }

        protected static HttpWebRequest CreateRequest(string uriStr)
        {
            var request = WebRequest.CreateHttp(Uri.EscapeUriString(uriStr));
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }

        protected async Task<string> ProcessRequestAsync(WebRequest request)
        {
            var timer = Stopwatch.StartNew();
            using (var response = await request.GetResponseAsync())
            {
                var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
                Log.InfoFormat($"Response from {request.RequestUri} received in {timer.ElapsedMilliseconds} ms");
                return result;
            }
        }

        protected async Task<string> ProcessRequestAsync(WebRequest webRequest, TimeSpan timeSpan)
        {
            var task = ProcessRequestAsync(webRequest);
            var completedTask = await Task.WhenAny(task, Task.Delay(timeSpan));

            if (completedTask != task)
            {
                throw new TimeoutException();
            }

            return await task;
        }

        protected static string CombineQueryUri(string uri,string query)
        {
            return uri + "?query=" + query;
        }
    }
}