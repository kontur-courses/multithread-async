using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> replicaResponseTimes = new();

        protected ClusterClientBase(string[] replicaAddresses)
        {
            ReplicaAddresses = replicaAddresses;
            
            foreach (var replica in replicaAddresses)
            {
                replicaResponseTimes[replica] = new ConcurrentQueue<long>();
            }
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
                UpdateResponseTime(request.RequestUri.ToString(), timer.ElapsedMilliseconds);
                return result;
            }
        }
        
        private void UpdateResponseTime(string replica, long responseTime)
        {
            var times = replicaResponseTimes.GetOrAdd(replica, _ => new ConcurrentQueue<long>());
    
            times.Enqueue(responseTime);
    
            if (times.Count > 10)
            {
                times.TryDequeue(out _);
            }
        }
        
        protected List<string> GetSortedReplicasBySpeed()
        {
            var sortedReplicas = replicaResponseTimes
                .OrderBy(kv =>
                {
                    if (kv.Value.IsEmpty)
                        return 0;

                    var averageResponseTime = kv.Value.DefaultIfEmpty(0).Average();
                    return averageResponseTime;
                })
                .Select(kv => kv.Key)
                .ToList();
            
            if (replicaResponseTimes.All(kv => kv.Value.IsEmpty))
                return ReplicaAddresses.ToList();

            Log.InfoFormat("Replica order: {0}", string.Join(", ", sortedReplicas));
            return sortedReplicas;
        }
    }
}