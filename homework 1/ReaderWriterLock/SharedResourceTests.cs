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
    private static readonly ManualResetEvent Event = new(false);

    [Test]
    public void TestConcurrentReadWrite() =>
        TestConcurrent(new SharedResourceLock());

    [Test]
    public void TestConcurrentReadWriteRwLock() =>
        TestConcurrent(new SharedResourceRwLock());

    private static void TestConcurrent(SharedResourceBase sharedResource)
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
        data
            .Select(text => new Thread(() =>
            {
                Event.WaitOne();
                sharedResource.Write(text);
            }));

    private static IEnumerable<Thread> InitializeReaders(SharedResourceBase sharedResource) =>
        Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(() =>
            {
                Event.WaitOne();
                sharedResource.Read();
            }));

    private static void ExecuteThreads(Thread[] threads)
    {
        threads.ForEach(t => t.Start());
        Event.Set();
        threads.ForEach(t => t.Join());
    }
}