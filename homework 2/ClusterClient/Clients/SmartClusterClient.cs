using System;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient : ClusterClientBase
{
    public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
    {
    }

    public override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
}