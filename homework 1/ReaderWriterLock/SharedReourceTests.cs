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

    [Test]
    public void TestConcurrentReadWrite()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read());
    }
}