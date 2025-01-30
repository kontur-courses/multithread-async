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
    public void TestConcurrentReadWrite()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        var shared = new SharedResourceLock();
        ConcurrentReadWrite(shared, WritersThreads, ReadersThreads);
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        var shared = new SharedResourceLock();
        ConcurrentReadWrite(shared, WritersThreads, ReadersThreads);    }

    private void ConcurrentReadWrite(SharedResourceBase sharedResource, int writeCount, int readCount)
    {
        var readers = Enumerable.Range(0, readCount)
            .Select(_ => new Thread(() => sharedResource.Read())).ToList();
        var writers = Enumerable.Range(0, writeCount)
            .Select(i => new Thread(() => sharedResource.Write(i.ToString())));
        readers.AddRange(writers);
        readers.ForEach(reader => reader.Start());
        readers.ForEach(reader => reader.Join());
        sharedResource.Read().Should().BeEquivalentTo($"{writeCount - 1}");
    }
}