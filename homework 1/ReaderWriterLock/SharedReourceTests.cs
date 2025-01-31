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
    public void TestConcurrentReadWrite()
    {
        Check(new SharedResourceLock());
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        Check(new SharedResourceRwLock());
    }

    private void Check(SharedResourceBase sharedResource)
    {
        var writers = Enumerable.Range(0, WritersThreads)
            .Select(number => new Thread(() => sharedResource.Write($"{number}")));
        var readers =  Enumerable.Range(0, ReadersThreads)
            .Select(number => new Thread(() => sharedResource.Read()));
        var threads = writers.Concat(readers).ToArray();
        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());
        
        sharedResource.Read().Should().Be($"{WritersThreads - 1}");
    }
}