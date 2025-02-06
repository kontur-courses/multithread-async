using System.Collections.Generic;

namespace ClusterClient.ReplicasPriorityManagers
{
    public class ReplicaStatisticsComparerByAverage : IComparer<ReplicaStatistics>
    {
        public int Compare(ReplicaStatistics x, ReplicaStatistics y)
        {
            var averageResponseTime1 = x.GetAverageResponseTime();
            var averageResponseTime2 = y.GetAverageResponseTime();
            return averageResponseTime1.CompareTo(averageResponseTime2);
        }
    }
}
