using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : UpdatedClusterClientBase(replicaAddresses)
{
    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var list = new List<Task<string>>();
        var interval = timeout.Divide(ReplicaAddresses.Length);
        var cts = new CancellationTokenSource(timeout);
        
        foreach (var address in ReplicaAddresses)
        {
            var uri = address + "?query=" + query;
            Log.InfoFormat($"Processing {uri}");
            list.Add(Get(uri, cts.Token));
            await Task.Delay(interval);
            var resultTask = await Task.WhenAny(list);
            list.Remove(resultTask);
            if (resultTask.IsCanceled)
                throw new TimeoutException();
            if (resultTask.IsFaulted || !resultTask.IsCompleted) continue;
            
            return await resultTask;;
        }   
        
        throw new TimeoutException();
    }
}