using System.Linq;
using ClusterClient.Clients;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterTests
{
	public class RandomClusterClientTest : ClusterTest
	{
		protected override ClusterClientBase CreateClient(string[] replicaAddresses)
			=> new RandomClusterClient(replicaAddresses);

		[Test]
		public void ClientShouldReturnSuccessIn50Percent()
		{
			CreateServer(1, status:500);
			CreateServer(1);

			Enumerable.Range(0, 200)
				.Select(_ =>
				{
					try
					{
						ProcessRequests(Timeout, 1);
						return 1;
					}
					catch
					{
						return 0;
					}
				})
				.Sum().Should().BeCloseTo(100, 20);
		}
	}
}