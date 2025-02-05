using ClusterClient.ReplicasPriorityManagers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ClusterTests.ReplicasPriorityManagersTests
{
    [TestFixture]
    internal class ReplicaStatisticsTest
    {
        private ReplicaStatistics replicaStatistics;

        [SetUp]
        public void SetUp()
        {
            replicaStatistics = new ReplicaStatistics();
        }

        [Test]
        public void Add_IncrementsRequestCountAndAddsTime()
        {
            var timeToAdd = TimeSpan.FromMilliseconds(150);
            var iterationCount = 3;

            for (var i = 0; i < iterationCount; i++)
            {
                replicaStatistics.Add(timeToAdd);
            }

            replicaStatistics.TotalTime.Should().Be(timeToAdd * iterationCount);
            replicaStatistics.RequestsCount.Should().Be(iterationCount);
        }

        [Test]
        public void Set_IncrementsRequestCountAndSetTime()
        {
            var timeToSet = TimeSpan.FromMilliseconds(150);
            var iterationCount = 3;

            for (var i = 0; i < iterationCount; i++)
            {
                replicaStatistics.Set(timeToSet);
            }

            replicaStatistics.TotalTime.Should().Be(timeToSet);
            replicaStatistics.RequestsCount.Should().Be(iterationCount);
        }

        [Test]
        public void Set_ResetsTotalTimeAndIncrementsRequestCount()
        {
            var timeToAdd = TimeSpan.FromMilliseconds(100);
            var timeToSet = TimeSpan.FromMilliseconds(150);

            replicaStatistics.Add(timeToAdd);
            replicaStatistics.Set(timeToSet);

            replicaStatistics.TotalTime.Should().Be(timeToSet);
            replicaStatistics.RequestsCount.Should().Be(2);
        }

        [Test]
        public void Add_AddsTotalTimeAfterSetAndIncrementsRequestCount()
        {
            var timeToAdd = TimeSpan.FromMilliseconds(100);
            var timeToSet = TimeSpan.FromMilliseconds(150);

            replicaStatistics.Set(timeToSet);
            replicaStatistics.Add(timeToAdd);

            replicaStatistics.TotalTime.Should().Be(timeToAdd + timeToSet);
            replicaStatistics.RequestsCount.Should().Be(2);
        }

        [Test]
        public void GetAverageResponseTime_ReturnsCorrectValue()
        {
            var timeToSet1 = TimeSpan.FromMilliseconds(150);
            var timeToSet2 = TimeSpan.FromMilliseconds(400);
            var timeToSet3 = TimeSpan.FromMilliseconds(350);
            var expected = (timeToSet1 + timeToSet2 + timeToSet3) / 3;

            replicaStatistics.Add(timeToSet1);
            replicaStatistics.Add(timeToSet2);
            replicaStatistics.Add(timeToSet3);

            replicaStatistics.GetAverageResponseTime().Should().Be(expected);
        }

        [Test]
        public void GetAverageResponseTime_ReturnsZero_WhenNoRequests()
        {
            replicaStatistics.GetAverageResponseTime().Should().Be(new TimeSpan());
        }

        [Test]
        public async Task Add_IsThreadSafe()
        {
            var timeToAdd = TimeSpan.FromMilliseconds(1);
            var iterationCount = 100;

            var tasks = new Task[iterationCount];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => replicaStatistics.Add(timeToAdd));
            }

            await Task.WhenAll(tasks);

            replicaStatistics.TotalTime.Should().Be(timeToAdd * iterationCount);
            replicaStatistics.RequestsCount.Should().Be(iterationCount);
        }

        [Test]
        public async Task Set_IsThreadSafe()
        {
            var timeToSet = TimeSpan.FromMilliseconds(1);
            var iterationCount = 100;

            var tasks = new Task[iterationCount];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => replicaStatistics.Set(timeToSet));
            }

            await Task.WhenAll(tasks);

            replicaStatistics.TotalTime.Should().Be(timeToSet);
            replicaStatistics.RequestsCount.Should().Be(iterationCount);
        }
    }
}

