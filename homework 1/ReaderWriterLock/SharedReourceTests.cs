using System.Collections.Generic;
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
    public void TestConcurrentReadWrite() =>
        TestConcurrentSharedResource(new SharedResourceLock());
    
    [Test]
    public void TestConcurrentReadWriteRwLock() =>
        TestConcurrentSharedResource(new SharedResourceRwLock());

    private void TestConcurrentSharedResource(SharedResourceBase sharedResource)
    {
        var threads = CreateThreads(sharedResource).ToArray();
        
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        sharedResource.Read().Should().BeEquivalentTo($"Data {WritersThreads - 1}");
    }

    private IEnumerable<Thread> CreateThreads(SharedResourceBase sharedResource)
    {
        var writingThreads = Enumerable
            .Range(0, WritersThreads)
            .Select(i => new Thread(() => sharedResource.Write($"Data {i}")));
        var readingThreads = Enumerable
            .Range(0, ReadersThreads)
            .Select(i => new Thread(() => sharedResource.Read()));
        
        return writingThreads.Concat(readingThreads);
    }
}