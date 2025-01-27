using System;
using System.Collections.Generic;
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

    [Test]
    public void TestConcurrentReadWrite() =>
        TestConcurrent(new SharedResourceLock());

    [Test]
    public void TestConcurrentReadWriteRwLock() =>
        TestConcurrent(new SharedResourceRwLock());

    private void TestConcurrent(SharedResourceBase sharedResource)
    {
        var data = Enumerable.Range(0, WritersThreads)
            .Select(x => $"Thread {x} data. ")
            .ToArray();
        var writers = InitializeWriters(sharedResource, data);
        var readers = InitializeReaders(sharedResource);
        ExecuteThreads(readers.Concat(writers).ToArray());

        var actual = sharedResource.Read();
        var expected = data[^1];
        var expectedDataLength = data.Aggregate((x, y) => x + y).Length;

        actual.Length.Should().Be(expectedDataLength);
        actual.Should().Contain(expected);
    }

    private static IEnumerable<Thread> InitializeWriters(SharedResourceBase sharedResource, IEnumerable<string> data) =>
        data.Select(text => new Thread(() => sharedResource.Write(text)));

    private static IEnumerable<Thread> InitializeReaders(SharedResourceBase sharedResource) =>
        Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(() => sharedResource.Read()));

    private static void ExecuteThreads(Thread[] threads)
    {
        threads.AsParallel().ForAll(t => t.Start());
        threads.AsParallel().ForAll(t => t.Join());
    }
}