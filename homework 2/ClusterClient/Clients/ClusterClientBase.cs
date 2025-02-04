using System;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients;

public abstract class ClusterClientBase(string[] replicaAddresses)
{
    protected abstract ILog Log { get; }
    protected string[] ReplicaAddresses { get; set; } = replicaAddresses;
    public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout);
}