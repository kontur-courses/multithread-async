using System;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public class SmartClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
{
    protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));

    public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }
}