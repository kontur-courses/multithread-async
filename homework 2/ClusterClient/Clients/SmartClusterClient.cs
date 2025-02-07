using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly Dictionary<string, List<TimeSpan>> _replicaResponseTimes = new();

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var countOfReplicas = ReplicaAddresses.Length;
        var workingTasks = new List<Task<string>>();

        var sortedReplicas = ReplicaAddresses
            .Select(uri => new {Uri = uri, AvgResponseTime = GetAverageResponseTime(uri)})
            .OrderBy(x => x.AvgResponseTime)
            .ToList();

        foreach (var replica in sortedReplicas)
        {
            var uri = replica.Uri;
            var perReplicaTimeout = timeout / countOfReplicas--;
            var webRequest = CreateRequest(uri + "?query=" + query);
            var task = ProcessRequestAsync(webRequest);
            workingTasks.Add(task);

            var sw = Stopwatch.StartNew();
            var completedTask = await Task.WhenAny(workingTasks.Append(Task.Delay(perReplicaTimeout)));
            timeout -= TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);

            if (completedTask is not Task<string> resultTask) continue;
            if (resultTask.IsCompletedSuccessfully && resultTask.Result is not null)
            {
                UpdateResponseTime(uri, sw.Elapsed);
                return resultTask.Result;
            }

            workingTasks.Remove(resultTask);
        }

        throw new TimeoutException("Request to the replicas timed out.");
    }

    private TimeSpan GetAverageResponseTime(string replicaUri)
    {
        if (_replicaResponseTimes.TryGetValue(replicaUri, out List<TimeSpan> value) && value.Count > 0)
        {
            return TimeSpan.FromMilliseconds(value.Average(t => t.TotalMilliseconds));
        }

        return TimeSpan.MinValue;
    }

    private void UpdateResponseTime(string replicaUri, TimeSpan responseTime)
    {
        if (!_replicaResponseTimes.TryGetValue(replicaUri, out List<TimeSpan> value))
        {
            value = [];
            _replicaResponseTimes[replicaUri] = value;
        }

        value.Add(responseTime);
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}