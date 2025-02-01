using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public abstract class ClusterClientBase(string[] replicaAddresses)
{
    protected string[] ReplicaAddresses { get; } = replicaAddresses;

    public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken = default);

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
    
    protected async Task<string> ProcessRequestAsync(WebRequest request, CancellationToken cancellationToken = default)
    {
        var timer = Stopwatch.StartNew();
        using var response = await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        var result = await new StreamReader(stream, Encoding.UTF8).ReadToEndAsync(cancellationToken);
        Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
        return result;
    }

    protected static string BuildUri(string baseUri, string query)
    {
        return $"{baseUri}?query={query}";
    }
}