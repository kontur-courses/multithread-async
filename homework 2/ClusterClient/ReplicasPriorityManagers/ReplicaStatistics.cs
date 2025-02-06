using System;

namespace ClusterClient.ReplicasPriorityManagers
{
    public class ReplicaStatistics
    {
        public TimeSpan TotalTime { get; private set; }
        public int RequestsCount { get; private set; }
        private readonly object locker = new object();

        public void Add(TimeSpan responseTime)
        {
            lock (locker)
            {
                TotalTime += responseTime;
                RequestsCount += 1;
            }
        }

        public void Set(TimeSpan responseTime)
        {
            lock (locker)
            {
                TotalTime = responseTime;
                RequestsCount += 1;
            }
        }

        public TimeSpan GetAverageResponseTime()
        {
            lock (locker)
            {
                if (RequestsCount == 0)
                {
                    return new TimeSpan();
                }
                return TotalTime / RequestsCount;
            }
        }
    }
}
