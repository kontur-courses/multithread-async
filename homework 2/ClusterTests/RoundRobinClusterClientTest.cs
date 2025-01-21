using System;
using System.Diagnostics;
using System.Linq;
using ClusterClient.Clients;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterTests
{
	public class RoundRobinClusterClientTest : ClusterTest
	{
		protected override ClusterClientBase CreateClient(string[] replicaAddresses)
			=> new RoundRobinClusterClient(replicaAddresses);

		[Test]
		public override void Client_should_return_success_when_timeout_is_close()
		{
			for(int i = 0; i < 3; i++)
				CreateServer(Timeout / 3);

			ProcessRequests(Timeout + 100);
		}

		[Test]
		public void ShouldReturnSuccessWhenLastReplicaIsGoodAndOthersAreSlow()
		{
			for(int i = 0; i < 3; i++)
				CreateServer(Slow);
			CreateServer(Fast);

			ProcessRequests(Timeout).Last().Should().BeCloseTo(TimeSpan.FromMilliseconds(3 * Timeout / 4 + Fast), Epsilon);
		}

		[Test]
		public void ShouldReturnSuccessWhenLastReplicaIsGoodAndOthersAreBad()
		{
			for(int i = 0; i < 1; i++)
				CreateServer(1, status: 500);
			CreateServer(Fast);

			ProcessRequests(Timeout).Last().Should().BeCloseTo(TimeSpan.FromMilliseconds(Fast), Epsilon);
		}

		[Test]
		public void ShouldThrowAfterTimeout()
		{
			for(var i = 0; i < 10; i++)
				CreateServer(Slow);

			var sw = Stopwatch.StartNew();
			Assert.Throws<TimeoutException>(() => ProcessRequests(Timeout));
			sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(Timeout), Epsilon);
		}

		[Test]
		public void ShouldForgetPreviousAttemptWhenStartNew()
		{
			CreateServer(4500);
			CreateServer(3000);
			CreateServer(10000);

			var sw = Stopwatch.StartNew();
			Assert.Throws<TimeoutException>(() => ProcessRequests(6000));
			sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(Timeout), Epsilon);
		}

		[Test]
		public void ShouldNotSpendTimeOnBad()
		{
			CreateServer(1, status: 500);
			CreateServer(1, status: 500);
			CreateServer(10000);
            CreateServer(2500);

			foreach(var time in ProcessRequests(6000))
				time.Should().BeCloseTo(TimeSpan.FromMilliseconds(5500), Epsilon);
		}
	}
}