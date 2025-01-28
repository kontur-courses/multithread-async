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

    [Test]
    public void TestConcurrentReadWrite()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads

        ConcurrentSharedResourceTest(new SharedResourceLock());
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        
        // ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }

    private static void ConcurrentSharedResourceTest(SharedResourceBase sharedResource)
    {
        var writers = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(() => sharedResource.Write($"Data: {i}"))).ToList();

        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => sharedResource.Read())).ToList();
        
        writers.ForEach(w => w.Start());
        readers.ForEach(r => r.Start());
        
        writers.ForEach(w => w.Join());
        readers.ForEach(r => r.Join());
        
        Assert.That(sharedResource.Read(), Is.EqualTo($"Data: {WritersThreads - 1}"));
    }
}