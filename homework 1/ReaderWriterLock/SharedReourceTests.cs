using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using FluentAssertions;   

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
        var writeThreads = Enumerable.Range(1, WritersThreads)
            .Select(i => new Thread(_ => _sharedResource.Write($"Data {i}")));
        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(_ => _sharedResource.Read()));
        var threads = writeThreads.Concat(readThreads).ToList();

        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());

        _sharedResource.Read().Should().BeEquivalentTo($"Data {WritersThreads}");
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();
        var writeThreads = Enumerable.Range(1, WritersThreads)
            .Select(i => new Thread(_ => _sharedResource.Write($"Data {i}")));
        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(_ => _sharedResource.Read()));
        var threads = writeThreads.Concat(readThreads).ToList();

        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());

        _sharedResource.Read().Should().BeEquivalentTo($"Data {WritersThreads}");
    }
}