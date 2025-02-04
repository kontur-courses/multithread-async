using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Clients;

public abstract class UpdatedClusterClientBase(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    protected readonly HttpClient HttpClient = new();
    protected virtual async Task<string> Get(string uri, CancellationToken token)
    {
        uri = Uri.EscapeUriString(uri);
        var stream = await HttpClient.GetStreamAsync(uri, token);
        var timer = Stopwatch.StartNew(); 
        var result = await new StreamReader(stream, Encoding.UTF8).ReadToEndAsync(token);
        Log.InfoFormat("Response from {0} received in {1} ms", uri, timer.ElapsedMilliseconds);
        return result;
    }
}