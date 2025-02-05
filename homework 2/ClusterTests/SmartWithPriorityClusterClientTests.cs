using ClusterClient.Clients;
using ClusterClient.ReplicasPriorityManagers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace ClusterTests
{
    internal class SmartWithPriorityClusterClientTests : SmartClusterClientTest
    {
        private ResponseTimeManagerByAverage replicasPriorityByAverageResponseTimeManager;

        protected override ClusterClientBase CreateClient(string[] replicaAddresses)
            => new SmartWithPriorityClusterClient(
                replicaAddresses,
                replicasPriorityByAverageResponseTimeManager);

        [SetUp]
        public void RecreateTimeManager()
        {
            replicasPriorityByAverageResponseTimeManager = new ResponseTimeManagerByAverage();
        }

        [Test]
        public void Сlient_ShouldPrioritizeFastestReplica()
        {
            CreateServer(3_500);
            CreateServer(2_000);
            CreateServer(1_000);

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 2; i++)
            {
                ProcessRequests(Timeout);
            }
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(4_500), Epsilon);
        }

        [Test]
        public void Сlient_ShouldPrioritizeFastestReplicaWithBad()
        {
            CreateServer(500, status: 500);
            CreateServer(1_000);

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 2; i++)
            {
                ProcessRequests(Timeout);
            }
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(2_500), Epsilon);
        }
    }
}
