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
    private SharedResourceBase sharedResource;

    [Test]
    public void TestConcurrentReadWrite()
    {
        sharedResource = new SharedResourceLock();
        RunConcurrentReadWriteTest();
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        sharedResource = new SharedResourceRwLock();
        RunConcurrentReadWriteTest();
    }
    
    private void RunConcurrentReadWriteTest()
    {
        var writerThreads = Enumerable.Range(1, WritersThreads)
            .Select(i => new Thread(() => sharedResource.Write($"Data {i}")))
            .ToList();
        
        var readerThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => sharedResource.Read()))
            .ToList();
        
        var allThreads = writerThreads.Concat(readerThreads).ToList();
        
        allThreads.ForEach(thread => thread.Start());
        allThreads.ForEach(thread => thread.Join());
        
        sharedResource.Read().Should().BeEquivalentTo($"Data {WritersThreads}");
    }
}