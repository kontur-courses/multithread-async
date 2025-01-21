using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cluster;
using ClusterClient.Clients;
using FluentAssertions;
using log4net;
using log4net.Config;
using NUnit.Framework;

namespace ClusterTests
{
	[TestFixture]
	public abstract class ClusterTest
	{
		protected const int Slow = 10_000_000;
		protected const int Fast = 500;
		protected const int Fastest = 10;
		protected const int Timeout = 6_000;
		protected const int Epsilon = 300;

		[Test]
		public void Client_should_return_success_when_there_is_only_one_fast_replica()
		{
			CreateServer(Fast);

			ProcessRequests(Timeout);
		}

		[Test]
		public void Client_should_return_success_when_all_replicas_are_fast()
		{
			for(int i = 0; i < 3; i++)
				CreateServer(Fast);

			ProcessRequests(Timeout);
		}

		[Test]
		public virtual void Client_should_return_success_when_timeout_is_close()
		{
			for(int i = 0; i < 3; i++)
				CreateServer(Timeout);

			ProcessRequests(Timeout + Epsilon);
		}

		[Test]
		public void Client_should_timeout_when_all_replicas_are_slow()
		{
			for(int i = 0; i < 3; i++)
				CreateServer(Slow);

			Action action = () => ProcessRequests(Timeout);

			action.Should().Throw<TimeoutException>();
		}

		[Test]
		public void Client_should_fail_when_all_replicas_are_bad()
		{
			for(int i = 0; i < 3; i++)
				CreateServer(Fast, status: 500);

			Action action = () => ProcessRequests(Timeout);

			action.Should().Throw<Exception>();
		}

		protected abstract ClusterClientBase CreateClient(string[] replicaAddresses);

		[SetUp]
		public void SetUp() => clusterServers = new List<ClusterServer>();

		[TearDown]
		public void TearDown() => StopServers();

		protected ClusterServer CreateServer(int delay, bool notStart = false, int status = 200)
		{
			var serverOptions = new ServerOptions
			{
				Async = true, MethodDuration = delay, MethodName = "some_method", Status = status,
				Port = GetFreePort()
			};

			var server = new ClusterServer(serverOptions, log);
			clusterServers.Add(server);

			if(!notStart)
			{
				server.Start();
				Console.WriteLine($"Started server at port {serverOptions.Port}");
			}

			return server;
		}

		protected TimeSpan[] ProcessRequests(double timeout, int take = 20)
		{
			var addresses = clusterServers
				.Select(cs => $"http://127.0.0.1:{cs.ServerOptions.Port}/{cs.ServerOptions.MethodName}/")
				.ToArray();

			var client = CreateClient(addresses);

			Console.WriteLine("Testing {0} started", client.GetType());
			var result = Task.WhenAll(Enumerable.Range(0, take).Select(i => i.ToString("x8")).Select(
				async query =>
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
					catch(TimeoutException)
					{
						Console.WriteLine("Query \"{0}\" timeout ({1} ms)", query, timer.ElapsedMilliseconds);
						throw;
					}
				}).ToArray()).GetAwaiter().GetResult();
			Console.WriteLine("Testing {0} finished", client.GetType());
			return result;
		}


		private void StopServers()
		{
			foreach(var clusterServer in clusterServers)
				clusterServer.Stop();
		}

		private static int GetFreePort()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			try
			{
				listener.Start();
				return ((IPEndPoint)listener.LocalEndpoint).Port;
			}
			finally
			{
				listener.Stop();
			}
		}

		private List<ClusterServer> clusterServers;

		private readonly ILog log = LogManager.GetLogger(typeof(Program));

        static ClusterTest()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), new FileInfo("log4net.config"));
		}
	}
}