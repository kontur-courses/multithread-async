using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private List<int> threadsIdsOrder;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase sharedResource;
    private ManualResetEvent manualResetEvent;

    [SetUp]
    public void SetUp()
    {
        threadsIdsOrder = new List<int>();
        manualResetEvent = new ManualResetEvent(false);
    }

    [Test]
    [Repeat(1000)]
    public void TestConcurrentReadWrite()
    {
        sharedResource = new SharedResourceLock();

        var writers = CreateWritersThreads();
        var readers = CreateReadersThreads();

        manualResetEvent.Set();

        writers.ForEach(x => x.Join());
        readers.ForEach(x => x.Join());

        var sharedResourceAsLock = (SharedResourceLock)sharedResource;

        var values = sharedResourceAsLock.Values;
        values.Should().HaveCount(WritersThreads);
        for (var i = 0; i < threadsIdsOrder.Count; i++)
        {
            var expected = $"Data {threadsIdsOrder[i]}";
            var actual = values[i];
            actual.Should().Be(expected);
        }
    }

    [Test]
    [Repeat(1000)]
    public void TestConcurrentReadWriteRwLock()
    {
        sharedResource = new SharedResourceRwLock();

        var writers = CreateWritersThreads();
        var readers = CreateReadersThreads();

        manualResetEvent.Set();

        writers.ForEach(x => x.Join());
        readers.ForEach(x => x.Join());

        var sharedResourceAsRwLock = (SharedResourceRwLock)sharedResource;

        var values = sharedResourceAsRwLock.Values;
        values.Should().HaveCount(WritersThreads);
        for (var i = 0; i < threadsIdsOrder.Count; i++)
        {
            var expected = $"Data {threadsIdsOrder[i]}";
            var actual = values[i];
            actual.Should().Be(expected);
        }
    }

    private Thread[] CreateWritersThreads()
    {
        var result = new Thread[WritersThreads];
        for (var i = 0; i < WritersThreads; i++)
        {
            var threadId = i;
            result[i] = new Thread(() =>
            {
                manualResetEvent.WaitOne();
                lock (threadsIdsOrder)
                {
                    sharedResource.Write($"Data {threadId}");
                    threadsIdsOrder.Add(threadId);
                }
            });
            result[i].Start();
        }
        return result;
    }

    private Thread[] CreateReadersThreads()
    {
        var result = new Thread[ReadersThreads];
        for (var i = 0; i < ReadersThreads; i++)
        {
            var threadId = i;
            result[i] = new Thread(() =>
            {
                manualResetEvent.WaitOne();
                sharedResource.Read();
            });
            result[i].Start();
        }
        return result;
    }
}