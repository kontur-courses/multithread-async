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

    [Test]
    public void TestConcurrentReadWrite()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads

        _sharedResource = new SharedResourceLock();
        
        var writers = Enumerable
            .Range(0, WritersThreads)
            .Select(i => new Thread(() => _sharedResource.Write($"Data {i}")))
            .ToArray();
        
        var readers = Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(() => _sharedResource.Read()))
            .ToArray();
        
        writers.ForEach(t => t.Start());
        readers.ForEach(t => t.Start());
        writers.ForEach(t => t.Join());
        readers.ForEach(t => t.Join());
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        
        _sharedResource = new SharedResourceRwLock();
        
        var writers = Enumerable
            .Range(0, WritersThreads)
            .Select(i => new Thread(() => _sharedResource.Write($"Data {i}")))
            .ToArray();
        
        var readers = Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(() => _sharedResource.Read()))
            .ToArray();
        
        writers.ForEach(t => t.Start());
        readers.ForEach(t => t.Start());
        writers.ForEach(t => t.Join());
        readers.ForEach(t => t.Join());
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
}