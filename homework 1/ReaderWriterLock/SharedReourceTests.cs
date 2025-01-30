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
    private SharedResourceBase _sharedResource;

    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();
        TestSharedResource();
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();
        TestSharedResource();
    }

    private void TestSharedResource()
    {
        var threads = Enumerable
            .Range(0, WritersThreads)
            .Select(x => new Thread(() => _sharedResource.Write(x.ToString())))
            .Concat(
                Enumerable
                    .Range(0, ReadersThreads)
                    .Select(_ => new Thread(() => _sharedResource.Read())))
            .ToArray();
        
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        var expected = (WritersThreads - 1).ToString();
        _sharedResource.Read()[^expected.Length..].Should().BeEquivalentTo(expected);
    }
}