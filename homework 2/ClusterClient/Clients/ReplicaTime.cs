namespace ClusterClient.Clients;

public class ReplicaTime(long time, string address)
{
    public long Time { get; set; } = time;
    public string Address { get; } = address;
}