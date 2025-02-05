namespace ClusterClient.Clients;

public class ReplicaStats
{
    public double TotalTime { get; set; }

    public int Count { get; set; }

    public double Average => Count == 0 ? 0 : (double)TotalTime / Count;
}