using System;
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

    // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
    // Проверка должна быть многопоточной.
    // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();

        var writers = Enumerable.Range(0, WritersThreads)
            .Select(x => new Thread(() => _sharedResource.Write($"Data {x}")));
        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => _sharedResource.Read()));

        var allThreads = writers.Concat(readers).ToList();
        allThreads.ForEach(x => x.Start());
        allThreads.ForEach(x => x.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }

    // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
    // Проверка должна быть многопоточной
    // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();

        var writers = Enumerable.Range(0, WritersThreads)
            .Select(x => new Thread(() => _sharedResource.Write($"Data {x}")));
        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => _sharedResource.Read()));

        var allThreads = writers.Concat(readers).ToList();
        allThreads.ForEach(x => x.Start());
        allThreads.ForEach(x => x.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
}