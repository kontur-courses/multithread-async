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
    private SharedResourceBase _sharedResource;

    // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
    // Проверка должна быть многопоточной.
    // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();
        
        TestConcurrent();
    }

    // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
    // Проверка должна быть многопоточной
    // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();
        
        TestConcurrent();
    }

    private void TestConcurrent()
    {
        var writers = Enumerable.Range(0, WritersThreads)
            .Select(x => new Thread(() => _sharedResource.Write($"Data: {x}")));
        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => _sharedResource.Read()));
        var threads = readers.Concat(writers).ToList();
        
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
        
        _sharedResource.Read().Should().Be($"Data: {WritersThreads - 1}");
    }
}