using ClusterClient.ReplicasPriorityManagers;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace ClusterTests.ReplicasPriorityManagersTests
{
    [TestFixture]
    internal class ReplicaStatisticsComparerByAverageTests
    {
        private readonly ReplicaStatisticsComparerByAverage comparer
            = new ReplicaStatisticsComparerByAverage();

        [Test]
        public void Compare_ReturnsNegative_WhenFirstReplicaIsFaster()
        {
            var replica1 = new ReplicaStatistics();
            replica1.Set(TimeSpan.FromMilliseconds(100));

            var replica2 = new ReplicaStatistics();
            replica2.Set(TimeSpan.FromMilliseconds(500));

            comparer.Compare(replica1, replica2).Should().Be(-1);
        }

        [Test]
        public void Compare_ReturnsZero_WhenBothReplicasHaveSameTime()
        {
            var replica1 = new ReplicaStatistics();
            replica1.Set(TimeSpan.FromMilliseconds(100));

            var replica2 = new ReplicaStatistics();
            replica2.Set(TimeSpan.FromMilliseconds(100));

            comparer.Compare(replica1, replica2).Should().Be(0);
        }

        [Test]
        public void Compare_ReturnsPositive_WhenFirstReplicaIsSlower()
        {
            var replica1 = new ReplicaStatistics();
            replica1.Set(TimeSpan.FromMilliseconds(500));

            var replica2 = new ReplicaStatistics();
            replica2.Set(TimeSpan.FromMilliseconds(100));

            comparer.Compare(replica1, replica2).Should().Be(1);
        }

        [Test]
        public void Compare_ReturnsZero_WhenBothReplicasAreEmpty()
        {
            var comparer = new ReplicaStatisticsComparerByAverage();
            var replica1 = new ReplicaStatistics();
            var replica2 = new ReplicaStatistics();

            comparer.Compare(replica1, replica2).Should().Be(0);
        }
    }
}
