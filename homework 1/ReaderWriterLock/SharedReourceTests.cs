using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase _sharedResource;
    
    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();
        var allTreads = CreateTreads().ToArray();
        
        allTreads.ForEach(thread => thread.Start());
        allTreads.ForEach(thread => thread.Join());
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();
        var allTreads = CreateTreads().ToArray();
        
        allTreads.ForEach(thread => thread.Start());
        allTreads.ForEach(thread => thread.Join());
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }

    private IEnumerable<Thread> CreateTreads()
    {
        var writingThreads = Enumerable
            .Range(0, WritersThreads)
            .Select(num => new Thread(() => _sharedResource.Write($"Data {num}")));
        var readingThreads = Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(() => _sharedResource.Read()));
        
        return writingThreads.Concat(readingThreads);
    }
}