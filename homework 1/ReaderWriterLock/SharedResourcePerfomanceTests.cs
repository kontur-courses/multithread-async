using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 20000;
    private const int FactorialNumber = 200;

	[Test]
    public void TestLockPerformance()
    {
		var sharedResourceLock = new SharedResourceLock();
		long lockTime = MeasurePerformance(sharedResourceLock);
		Console.WriteLine($"Lock time taken: {lockTime} ms");

		var sharedResourceRW = new SharedResourceRwLock();
		long rwLockTime = MeasurePerformance(sharedResourceRW);
		Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

		rwLockTime.Should().BeLessThan(lockTime); // ReaderWriterLock should be faster than Lock
	}

    private long MeasurePerformance(SharedResourceBase sharedResource)
    {
		var writers = Enumerable.Range(0, WritersThreads)
			.Select(_ => new Thread(() => Write(sharedResource)));

		var readers = Enumerable.Range(0, ReadersThreads)
			.Select(_ => new Thread(() => Read(sharedResource)));

		var threads = writers.Concat(readers).ToArray();

		var sw = new Stopwatch();

		sw.Start();

		threads.ForEach(t => t.Start());
		threads.ForEach(t => t.Join());

		sw.Stop();

		return sw.ElapsedMilliseconds;
	}

	private void Write(SharedResourceBase sharedResource)
	{
		for (int i = 0; i < NumberOfIterations; i++)
		{
			sharedResource.Write($"Data {i}");
			sharedResource.ComputeFactorial(FactorialNumber);
		}
	}

	private void Read(SharedResourceBase sharedResource)
	{
		for (int i = 0; i < NumberOfIterations; i++)
		{
			sharedResource.Read();
			sharedResource.ComputeFactorial(FactorialNumber);
		}
	}
}