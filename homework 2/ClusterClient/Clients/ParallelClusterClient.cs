using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class ParallelClusterClient(string[] replicaAddresses) : UpdatedClusterClientBase(replicaAddresses)
{
    protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        Log.InfoFormat($"Parallel processing of all addresses with the query=\"{query}\" parameter");
        var cts = new CancellationTokenSource();
        var tasks =  ReplicaAddresses
            .Select(uri => GetData(uri + "?query=" + query, cts.Token))
            .ToList();
        
        cts.CancelAfter(timeout);
        while (tasks.Count != 0)
        {
            var resultTask = await Task.WhenAny(tasks);
            tasks.Remove(resultTask);
            if (resultTask.IsCanceled)
                throw new TimeoutException();
            if (resultTask.IsFaulted || !resultTask.IsCompleted) continue;
            
            return await resultTask;;
        }
        
        throw new TimeoutException();
    }
}