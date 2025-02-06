using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RoundRobinClusterClient(string[] replicaAddresses) : UpdatedClusterClientBase(replicaAddresses)
{
    protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var interval = timeout.Divide(ReplicaAddresses.Length);
        foreach (var address in ReplicaAddresses)
        {
            var uri = address + "?query=" + query;
            Log.InfoFormat($"Processing {uri}");
            var cts = new CancellationTokenSource();
            var resultTask = GetData(uri, cts.Token).WaitAsync(timeout);
            await Task.WhenAny(resultTask, Task.Delay(interval)); //
            if (!resultTask.IsFaulted && resultTask.IsCompleted)
                return await resultTask;
            
            cts.Cancel();
        }   
        
        throw new TimeoutException();
    }
}