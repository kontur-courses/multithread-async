namespace ClusterHistory.Models;

internal class ReplicaSendHistoryItem(string uri)
{
    private readonly object lockObject = new();
    private TimeSpan allTime;
    private int sendCount;
    public string Uri { get; } = uri;
    public int SendWithWorkTimeCount { get; private set; }
    public int SendCount => sendCount;

    public void IncrementSuccessfulSend(TimeSpan time)
    {
        lock (lockObject)
        {
            allTime += time;
            SendWithWorkTimeCount++;
        }
    }

    public void IncrementSendCount()
    {
        Interlocked.Increment(ref sendCount);
    }

    public TimeSpan CalculateAverageResponseTime()
    {
        if (SendWithWorkTimeCount == 0)
        {
            return TimeSpan.Zero;
        }

        lock (lockObject)
        {
            return allTime / SendWithWorkTimeCount;
        }
    }
}