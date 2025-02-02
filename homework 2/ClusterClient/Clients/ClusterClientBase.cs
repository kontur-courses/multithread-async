using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterHistory.Interfaces;
using log4net;

namespace ClusterClient.Clients;

public abstract class ClusterClientBase
{
    protected string[] ReplicaAddresses { get; }
    protected IReplicaSendHistory ReplicaSendHistory { get; }

    protected ClusterClientBase(string[] replicaAddresses, IReplicaSendHistory replicaSendHistory)
    {
        ReplicaAddresses = replicaAddresses;
        ReplicaSendHistory = replicaSendHistory;
    }

    public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken = default);

    protected abstract ILog Log { get; }

    protected static HttpWebRequest CreateRequest(Uri uri)
    {
        var request = WebRequest.CreateHttp(uri);
        request.Proxy = null;
        request.KeepAlive = true;
        request.ServicePoint.UseNagleAlgorithm = false;
        request.ServicePoint.ConnectionLimit = 100500;
        return request;
    }

    protected async Task<string> ProcessRequestAsync(WebRequest request, CancellationToken cancellationToken = default)
    {
        var isSearch = TrySearchReplicaAddress(request.RequestUri, out var replicaName);
        if (isSearch)
        {
            ReplicaSendHistory.AddSendAttempt(replicaName);
        }

        var timer = Stopwatch.StartNew();
        using var response = await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        var result = await new StreamReader(stream, Encoding.UTF8).ReadToEndAsync(cancellationToken);
        Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
        if (isSearch)
        {
            ReplicaSendHistory.AddWorkTime(replicaName, timer.Elapsed);
        }

        return result;
    }

    private bool TrySearchReplicaAddress(Uri uri, out string replicaName)
    {
        replicaName = ReplicaAddresses.FirstOrDefault(r => uri.ToString().Contains(r)) ?? string.Empty;
        return !string.IsNullOrEmpty(replicaName);
    }

    protected static Uri CreateUri(string baseUri, string query)
    {
        var uriStr = string.IsNullOrWhiteSpace(query) ? baseUri : $"{baseUri}?query={query}";
        var escapeUriString = Uri.EscapeUriString(uriStr);
        return new Uri(escapeUriString);
    }
}