using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public async override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var uriList = ReplicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .ToList();
            var timeForReplica = timeout / ReplicaAddresses.Length;
            Task<string> request = null;
            var delta = 0L;
            for (var i = 0; i < ReplicaAddresses.Length; i++)
            {
                request = ProcessRequestAsync(uriList[i]);
                var sw = Stopwatch.StartNew();
                var timerForReplica = Task.Delay(timeForReplica);
                var result = await Task.WhenAny(request, timerForReplica);
                if (result.IsFaulted)
                {
                    var remainingReplicaCount = ReplicaAddresses.Length - i - (i != ReplicaAddresses.Length - 1 ? 1 : 0);
                    timeForReplica = timeout.Add(new TimeSpan(-delta)) / remainingReplicaCount;
                }
                else
                {
                    delta += sw.ElapsedMilliseconds;
                    if (result != timerForReplica)
                        return await (result as Task<string>);
                    await Task.Factory.StartNew(() => SaveRequestHistory(sw.Elapsed, ReplicaAddresses[i]));
                }
            }
            if (!request.IsCompleted)
                throw new TimeoutException();
            return await request;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
