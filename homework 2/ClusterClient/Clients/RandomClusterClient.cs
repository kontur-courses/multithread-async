using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class RandomClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    private readonly Random _random = new();

    protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        var uri = ReplicaAddresses[_random.Next(ReplicaAddresses.Length)];

        var webRequest = CreateRequest(uri + "?query=" + query);

        Log.InfoFormat($"Processing {webRequest.RequestUri}");

        var resultTask = ProcessRequestAsync(webRequest);

        await Task.WhenAny(resultTask, Task.Delay(timeout));
        if (!resultTask.IsCompleted)
            throw new TimeoutException();

        return resultTask.Result;
    }
    
    private static HttpWebRequest CreateRequest(string uriStr)
    {
        var request = WebRequest.CreateHttp(Uri.EscapeUriString(uriStr));
        request.Proxy = null;
        request.KeepAlive = true;
        request.ServicePoint.UseNagleAlgorithm = false;
        request.ServicePoint.ConnectionLimit = 100500;
        return request;
    }

    private async Task<string> ProcessRequestAsync(WebRequest request)
    {
        var timer = Stopwatch.StartNew();
        using var response = await request.GetResponseAsync();
        var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
        Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
        return result;
    }
}