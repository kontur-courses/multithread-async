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
    [Repeat(50)]
    public void TestConcurrentReadWrite()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        
        TestSharedResource(new SharedResourceLock());
    }
    
    [Test]
    [Repeat(50)]
    public void TestConcurrentReadWriteRwLock()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads

        TestSharedResource(new SharedResourceRwLock());
    }

    private void TestSharedResource(SharedResourceBase sharedResource)
    {
        var writeThreads = CreateWriteThreads(sharedResource);
        var readThreads = CreateReadThreads(sharedResource);
        
        writeThreads.ForEach(thread => thread.Start());
        readThreads.ForEach(thread => thread.Start());

        writeThreads.ForEach(thread => thread.Join());
        readThreads.ForEach(thread => thread.Join());
        
        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", sharedResource.Read());
    }

    private Thread[] CreateReadThreads(SharedResourceBase sharedResource)
    {
        var result = new Thread[ReadersThreads];

        for (int i = 0; i < ReadersThreads; i++)
        {
            result[i] = new Thread(() =>
            {
                sharedResource.Read();
            });
        }

        return result;
    }

    private Thread[] CreateWriteThreads(SharedResourceBase sharedResource)
    {
        var result = new Thread[WritersThreads];

        for (int i = 0; i < WritersThreads; i++)
        {
            var data = $"Data {i}";

            result[i] = new Thread(() =>
            {
                sharedResource.Write(data);
            });
        }

        return result;
    }
}