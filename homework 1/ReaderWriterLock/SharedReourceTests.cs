using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
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
    public void TestConcurrentReadWrite() =>
        TestConcurrent(new SharedResourceLock());

    [Test]
    public void TestConcurrentReadWriteRwLock() =>
        TestConcurrent(new SharedResourceRwLock());

    private void TestConcurrent(SharedResourceBase sharedResource)
    {
        _sharedResource = sharedResource;
        var writers = Enumerable.Range(0, WritersThreads)
            .Select(x => new Thread(() => _sharedResource.Write($"Data {x}")))
            .ToArray();
        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(x => new Thread(() => _sharedResource.Read()))
            .ToArray();
        var threads = writers.Concat(readers).ToArray();
        var expected = $"Data {WritersThreads - 1}";
        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());
        
        var actual = sharedResource.Read();
        var startPosition = actual.Length - expected.Length;
        var endOfActual = actual.Substring(startPosition, expected.Length);
        
        endOfActual.Should().BeEquivalentTo(expected);
    }
}