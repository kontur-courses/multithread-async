using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ReaderWriterLock;

public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase _sharedResource;

    [Test]
    public void TestConcurrentReadWrite()
    {
        var expected = $"Data {WritersThreads - 1}";

        var actual = GetActualSharedResources(
            new SharedResourceLock());

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        var expected = $"Data {WritersThreads - 1}";

        var actual = GetActualSharedResources(
            new SharedResourceRwLock());

        actual.Should().BeEquivalentTo(expected);
    }

    private string GetActualSharedResources(SharedResourceBase sharedResource)
    {
        _sharedResource = sharedResource;
        var writers = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(() => { _sharedResource.Write($"Data {i}"); }))
            .ToArray();
        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => { _sharedResource.Read(); }))
            .ToArray();

        writers.ForEach(writer => writer.Start());
        readers.ForEach(reader => reader.Start());

        writers.ForEach(writer => writer.Join());
        readers.ForEach(reader => reader.Join());

        return _sharedResource.Read();
    }
}