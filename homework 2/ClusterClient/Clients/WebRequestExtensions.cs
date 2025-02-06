using System.Net;

namespace ClusterClient.Clients;

public static class WebRequestExtensions
{
    public static string RequestPath(this WebRequest request)
    {
        var uri = request.RequestUri;
        return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
    }
}