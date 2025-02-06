using ClusterClient.ReplicasPriorityManagers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ClusterTests.ReplicasPriorityManagersTests
{
    [TestFixture]
    internal class ResponseTimeManagerByAverageTests
    {
        private ResponseTimeManagerByAverage replicasManager;

        [SetUp]
        public void SetUp()
        {
            replicasManager = new ResponseTimeManagerByAverage();
        }

        [Test]
        public void SortReplicasAddresses_SortsByAverageResponseTime()
        {
            var expected = new string[]
            {
                "replica2",
                "replica3",
                "replica1"
            };

            replicasManager.SetReplicaStatsTime("replica1", TimeSpan.FromMilliseconds(300));
            replicasManager.SetReplicaStatsTime("replica2", TimeSpan.FromMilliseconds(100));
            replicasManager.SetReplicaStatsTime("replica3", TimeSpan.FromMilliseconds(200));

            var actual = replicasManager.SortReplicasAddresses(new[] { "replica1", "replica2", "replica3" });

            actual.Should().Equal(expected);
        }

        [Test]
        public void SortReplicasAddresses_WorksWithUnknownReplicas()
        {
            var expected = new string[]
            {
                "replica2",
                "replica1"
            };
            replicasManager.SetReplicaStatsTime("replica1", TimeSpan.FromMilliseconds(200));

            var actual = replicasManager.SortReplicasAddresses(new[] { "replica1", "replica2" });

            actual.Should().Equal(expected);
        }

        [Test]
        public void SetReplicaStatsTime_SetsReplicaStatisticsCorrectly_WithNewReplica()
        {
            var replicaAddress = "replica1";
            var time = TimeSpan.FromMilliseconds(100);

            replicasManager.SetReplicaStatsTime(replicaAddress, time);

            var expected = replicasManager.GetReplicasStats;
            expected.Should().HaveCount(1);
            expected.Should().ContainKey(replicaAddress);
            expected[replicaAddress].RequestsCount.Should().Be(1);
            expected[replicaAddress].TotalTime.Should().Be(time);
        }

        [Test]
        public void SetReplicaStatsTime_SetsReplicaStatisticsCorrectly_WithOldReplica()
        {
            var replicaAddress = "replica1";
            var time = TimeSpan.FromMilliseconds(100);
            var iterationCount = 3;

            for (var i = 0; i < iterationCount; i++)
            {
                replicasManager.SetReplicaStatsTime(replicaAddress, time);
            }

            var expected = replicasManager.GetReplicasStats;
            expected.Should().HaveCount(1);
            expected.Should().ContainKey(replicaAddress);
            expected[replicaAddress].RequestsCount.Should().Be(iterationCount);
            expected[replicaAddress].TotalTime.Should().Be(time);
        }

        [Test]
        public void AddToReplicaStatsTime_AddsReplicaStatisticsCorrectly_WithNewReplica()
        {
            var replicaAddress = "replica1";
            var time = TimeSpan.FromMilliseconds(100);

            replicasManager.AddToReplicaStatsTime(replicaAddress, time);

            var expected = replicasManager.GetReplicasStats;
            expected.Should().HaveCount(1);
            expected.Should().ContainKey(replicaAddress);
            expected[replicaAddress].RequestsCount.Should().Be(1);
            expected[replicaAddress].TotalTime.Should().Be(time);
        }

        [Test]
        public void AddToReplicaStatsTime_AddsReplicaStatisticsCorrectly_WithOldReplica()
        {
            var replicaAddress = "replica1";
            var time = TimeSpan.FromMilliseconds(150);
            var iterationCount = 3;

            for (var i = 0; i < iterationCount; i++)
            {
                replicasManager.AddToReplicaStatsTime(replicaAddress, time);
            }

            var expected = replicasManager.GetReplicasStats;
            expected.Should().HaveCount(1);
            expected.Should().ContainKey(replicaAddress);
            expected[replicaAddress].RequestsCount.Should().Be(iterationCount);
            expected[replicaAddress].TotalTime.Should().Be(iterationCount * time);
        }

        [Test]
        public async Task SetReplicaStatsTime_IsThreadSafe()
        {
            var replicaAddress = "replica1";
            var timeToSet = TimeSpan.FromMilliseconds(1);
            var iterationCount = 100;

            var tasks = new Task[iterationCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => replicasManager.SetReplicaStatsTime(replicaAddress, timeToSet));
            }

            await Task.WhenAll(tasks);

            var actual = replicasManager.GetReplicasStats[replicaAddress];
            actual.TotalTime.Should().Be(timeToSet);
            actual.RequestsCount.Should().Be(iterationCount);
        }

        [Test]
        public async Task AddToReplicaStatsTime_IsThreadSafe()
        {
            var replicaAddress = "replica1";
            var timeToAdd = TimeSpan.FromMilliseconds(1);
            var iterationCount = 100;

            var tasks = new Task[iterationCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => replicasManager.AddToReplicaStatsTime(replicaAddress, timeToAdd));
            }

            await Task.WhenAll(tasks);

            var actual = replicasManager.GetReplicasStats[replicaAddress];
            actual.TotalTime.Should().Be(timeToAdd * iterationCount);
            actual.RequestsCount.Should().Be(iterationCount);
        }
    }
}
