using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cluster;
using ClusterClient.Clients;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterTests;

public class AdvancedRoundRobinClientTests : RoundRobinClusterClientTest
{
    protected override ClusterClientBase CreateClient(string[] replicaAddresses)
        => new AdvancedRoundRobinClusterClient(replicaAddresses);
    
    [Test]
    public void ShouldSortReplicasByReplyTime()
    {
        CreateServer(4000);
        CreateServer(3000);
        CreateServer(3500);
        CreateServer(1000);

        const int timeout = 6000;
        var client = CreateClient();
        ProcessRequest(client, "first", timeout).Should().BeCloseTo(TimeSpan.FromMilliseconds(5500), Epsilon);
        ProcessRequest(client, "second", timeout).Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), Epsilon);
    }

    [Test]
    public void ShouldAskBadReplicasLast()
    {
        CreateServer(200, status: 500);
        CreateServer(300, status: 500);
        CreateServer(10000);
        CreateServer(1300);
        
        const int timeout = 6000;
        var client = CreateClient();
        ProcessRequest(client, "first", timeout).Should().BeCloseTo(TimeSpan.FromMilliseconds(4500), Epsilon);
        ProcessRequest(client, "second", timeout).Should().BeCloseTo(TimeSpan.FromMilliseconds(1300), Epsilon);
    }

    [Test]
    public void ShouldTryUnexploredReplicasFirst()
    {
        CreateServer(4000);
        CreateServer(3000);
        CreateServer(1200);
        CreateServer(100);

        const int timeout = 6000;
        var client = CreateClient();
        ProcessRequest(client, "first", timeout).Should().BeCloseTo(TimeSpan.FromMilliseconds(4200), Epsilon);
        ProcessRequest(client, "second", timeout).Should().BeCloseTo(TimeSpan.FromMilliseconds(100), Epsilon);
    }

    private ClusterClientBase CreateClient()
    {
        return CreateClient(ClusterServers
            .Select(cs => $"http://127.0.0.1:{cs.ServerOptions.Port}/{cs.ServerOptions.MethodName}/")
            .ToArray());
    }

    private TimeSpan ProcessRequest(ClusterClientBase client, string query, double timeout, int take = 20)
    {
        return Task.Run(async () =>
        {
            var timer = Stopwatch.StartNew();
            try
            {
                var clientResult = await client.ProcessRequestAsync(query, TimeSpan.FromMilliseconds(timeout));
                timer.Stop();

                clientResult.Should().Be(Encoding.UTF8.GetString(ClusterHelpers.GetBase64HashBytes(query)));
                timer.ElapsedMilliseconds.Should().BeLessThan((long)timeout + Epsilon);

                Console.WriteLine("Query \"{0}\" successful ({1} ms)", query, timer.ElapsedMilliseconds);

                return timer.Elapsed;
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Query \"{0}\" timeout ({1} ms)", query, timer.ElapsedMilliseconds);
                throw;
            }
        }).GetAwaiter().GetResult();
    }
}