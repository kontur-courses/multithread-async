using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;

    [Test]
    public void TestConcurrentReadWriteLock()
    {
        var sharedResource = new SharedResourceLock();

		TestConcurentReadWrite(sharedResource, ReadersThreads, WritersThreads);
	}
    
    [Test]
	public void TestConcurrentReadWriteRwLock()
    {
		var sharedResource = new SharedResourceRwLock();

		TestConcurentReadWrite(sharedResource, ReadersThreads, WritersThreads);
	}

	public void TestConcurentReadWrite(SharedResourceBase sharedResource, int readersCount, int writersCount)
	{
		var readers = Enumerable.Range(0, readersCount)
			.Select(_ => new Thread(() => sharedResource.Read()));

		var writers = Enumerable.Range(0, writersCount)
			.Select(i => $"Data {i}")
			.Select(data => new Thread(() => sharedResource.Write(data)));

		var threads = writers.Concat(readers).ToArray();

		threads.ForEach(thread => thread.Start());
		threads.ForEach(thread => thread.Join());

		sharedResource.Read().Should().BeEquivalentTo($"Data {writersCount - 1}");
	}
}