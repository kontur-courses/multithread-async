using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public abstract class ClusterClientBase
    {
        protected string[] ReplicaAddresses { get; set; }
        protected Dictionary<string, (TimeSpan, int)> history = new();
        private object lockObject = new object();

        protected ClusterClientBase(string[] replicaAddresses)
        {
            ReplicaAddresses = replicaAddresses;
            history = ReplicaAddresses.ToDictionary(k => k, k => (new TimeSpan(0), 0));
        }

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
                Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
                return result;
            }
        }

        protected void SaveRequestHistory(TimeSpan requestTime, string replica)
        {
            lock (lockObject)
            {
                var averageTime = (history[replica].Item1 * history[replica].Item2 + requestTime) / (history[replica].Item2 + 1);
                history[replica] = (averageTime, history[replica].Item2 + 1);
                ReplicaAddresses = history.OrderBy(t => t.Value.Item1).Select(kvp => kvp.Key).ToArray();
            }
        }

        protected IEnumerable<string> GetAddresses()
        {
            return history.OrderBy(t => t.Value.Item1).Select(kvp => kvp.Key);
        }
    }
}