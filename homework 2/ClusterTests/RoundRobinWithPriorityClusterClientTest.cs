using ClusterClient.Clients;
using ClusterClient.ReplicasPriorityManagers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace ClusterTests
{
    internal class RoundRobinWithPriorityClusterClientTest : RoundRobinClusterClientTest
    {
        private ResponseTimeManagerByAverage replicasPriorityByAverageResponseTimeManager;

        protected override ClusterClientBase CreateClient(string[] replicaAddresses)
            => new RoundRobinWithPriorityClusterClient(
                replicaAddresses,
                replicasPriorityByAverageResponseTimeManager);

        [SetUp]
        public void RecreateTimeManager()
        {
            replicasPriorityByAverageResponseTimeManager = new ResponseTimeManagerByAverage();
        }

        [Test]
        public void Сlient_ShouldPrioritizeFasterReplica()
        {
            CreateServer(Slow);
            CreateServer(Fast);

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 2; i++)
            {
                ProcessRequests(Timeout);
            }
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(4_000), Epsilon);
        }

        [Test]
        public void Сlient_ShouldPrioritizeFasterReplicaWithBad()
        {
            CreateServer(Fast, status: 500);
            CreateServer(Fast);

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 2; i++)
            {
                ProcessRequests(Timeout);
            }
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(1_500), Epsilon);
        }
    }
}
