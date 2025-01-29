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
    public void TestConcurrentReadWrite() => ConcurrentReadWriteTest(new SharedResourceLock());
    
    [Test]
    public void TestConcurrentReadWriteRwLock() => ConcurrentReadWriteTest(new SharedResourceRwLock());

    private static void ConcurrentReadWriteTest(SharedResourceBase sharedResource)
    {
        var writers = CreateThreads(WritersThreads, i => sharedResource.Write($"Data {i}"));
        var readers = CreateThreads(ReadersThreads, _ => sharedResource.Read());
        
        writers.ForEach(writer => writer.Start());
        readers.ForEach(reader => reader.Start());
        
        writers.ForEach(writer => writer.Join());
        readers.ForEach(writer => writer.Join());

        sharedResource.Read().Should().Be($"Data {WritersThreads - 1}");
    }

    private static List<Thread> CreateThreads(int count, Action<int> action)
        => Enumerable.Range(0, count).Select(i => new Thread(() => action(i))).ToList();
}