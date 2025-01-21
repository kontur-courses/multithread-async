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
    private ISharedResource _sharedResource;

    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResource();
        var threads = Enumerable.Range(0, WritersThreads).Select(i => new Thread(() => _sharedResource.Write("Data " + i))).ToArray();
        threads = threads.Concat(
            Enumerable.Range(0, ReadersThreads).Select(i => new Thread(() =>
            {
                var data = _sharedResource.Read();
                Console.WriteLine("Read data: " + data);
            })).ToArray()
        ).ToArray();

        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());
        
        // Удалить все выше. Реализация теста конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRWLock();
        var threads = Enumerable.Range(0, WritersThreads).Select(i => new Thread(() => _sharedResource.Write("Data " + i))).ToArray();
        threads = threads.Concat(
            Enumerable.Range(0, ReadersThreads).Select(i => new Thread(() =>
            {
                var data = _sharedResource.Read();
                Console.WriteLine("Read data: " + data);
            })).ToArray()
        ).ToArray();

        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());
        
        // Удалить все выше. Реализация теста конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
}