using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public async override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var uriList = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .ToList();
            var timeForReplica = timeout / uriList.Count;
            Task<string> request = null;
            var requests = new List<Task<string>>();
            var delta = 0L;
            for (var i = 0; i < uriList.Count; i++)
            {
                var sw = Stopwatch.StartNew();
                var timerForReplica = Task.Delay(timeForReplica);
                var currentRequest = ProcessRequestAsync(uriList[i]);
                var result = requests.Count > 0
                    ? await Task.WhenAny(requests.Append(timerForReplica).Append(currentRequest))
                    : await Task.WhenAny(currentRequest, timerForReplica);
                if (result.IsFaulted)
                {
                    var remainingReplicaCount = uriList.Count - i - (i != uriList.Count - 1 ? 1 : 0);
                    timeForReplica = timeout.Add(new TimeSpan(-delta)) / remainingReplicaCount;
                }
                else
                {
                    delta += sw.ElapsedMilliseconds;
                    if (result != timerForReplica)
                        return await (result as Task<string>);
                    requests.Add(currentRequest);
                    await Task.Factory.StartNew(() => SaveRequestHistory(sw.Elapsed, ReplicaAddresses[i]));
                }
                request = currentRequest;
            }
            if (!request.IsCompleted)
                throw new TimeoutException();
            return await request;


            //var uriList = ReplicaAddresses
            //    .Select(uri => CreateRequest(uri + "?query=" + query))
            //    .ToList();
            //var timeForReplica = timeout / ReplicaAddresses.Length;
            //Task<string> request = null;
            //var requests = new List<Task<string>>();
            //var delta = 0L;
            //for (var i = 0; i < ReplicaAddresses.Length; i++)
            //{
            //    var sw = Stopwatch.StartNew();
            //    var timerForReplica = Task.Delay(timeForReplica);
            //    var currentRequest = ProcessRequestAsync(uriList[i]);                
            //    var result = requests.Count > 0 
            //        ? await Task.WhenAny(requests.Append(timerForReplica)) 
            //        : await Task.WhenAny(currentRequest, timerForReplica);
            //    if (result.IsFaulted)
            //    {
            //        var remainingReplicaCount = ReplicaAddresses.Length - i - (i != ReplicaAddresses.Length - 1 ? 1 : 0);
            //        timeForReplica = timeout.Add(new TimeSpan(-delta)) / remainingReplicaCount;
            //    }
            //    else
            //    {
            //        delta += sw.ElapsedMilliseconds;
            //        if (result != timerForReplica)
            //            return await(result as Task<string>);
            //        requests.Add(currentRequest);
            //    }
            //    request = currentRequest;
            //}
            //if (!request.IsCompleted)
            //    throw new TimeoutException();
            //return await request;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
