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
        var writeThreads = Enumerable.Range(1, WritersThreads)
            .Select(i => new Thread(_ => sharedResource.Write($"Data {i}")));
        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(_ => sharedResource.Read()));
        var threads = writeThreads.Concat(readThreads).ToList();

        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());

        sharedResource.Read().Should().BeEquivalentTo($"Data {WritersThreads}");
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        sharedResource = new SharedResourceRwLock();
        var writeThreads = Enumerable.Range(1, WritersThreads)
            .Select(i => new Thread(_ => sharedResource.Write($"Data {i}")));
        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(_ => sharedResource.Read()));
        var threads = writeThreads.Concat(readThreads).ToList();

        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());

        sharedResource.Read().Should().BeEquivalentTo($"Data {WritersThreads}");
    }
}