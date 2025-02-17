using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase _sharedResource;
    private SharedResourceLock _sharedResourceLock;
    private SharedResourceRwLock _sharedResourceRw;

    [SetUp]
    public void SetUp()
    {
        _sharedResourceLock = new SharedResourceLock();
        _sharedResourceRw = new SharedResourceRwLock();
    }

    [Test]
    public void TestConcurrentReadWrite()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        var writeThreads = new List<Thread>();
        var readThreads = new List<Thread>();

        for (int i = 0; i < WritersThreads; i++)
        {
            var j = i;
            writeThreads.Add(new Thread(_ => _sharedResourceLock.Write($"Data {j}")));
        }

        for (int i = 0; i < ReadersThreads; i++)
        {
            readThreads.Add(new Thread(_ => _sharedResourceLock.Read()));
        }
        var allThreads = writeThreads.Concat(readThreads);
        allThreads.ForEach(thread => thread.Start());
        allThreads.ForEach(thread => thread.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", _sharedResourceLock.Read());
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads

        var writeThreads = new List<Thread>();
        var readThreads = new List<Thread>();

        for (int i = 0; i < WritersThreads; i++)
        {
            writeThreads.Add(new Thread(_ => _sharedResourceRw.Write($"Data {i}")));
        }

        for (int i = 0; i < ReadersThreads; i++)
        {
            readThreads.Add(new Thread(_ => _sharedResourceRw.Read()));
        }
        var allThreads = writeThreads.Concat(readThreads);
        allThreads.ForEach(thread => thread.Start());
        allThreads.ForEach(thread => thread.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads}", _sharedResourceRw.Read());
    }
}