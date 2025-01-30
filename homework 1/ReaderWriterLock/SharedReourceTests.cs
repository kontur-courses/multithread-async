using System;
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
    private SharedResourceBase _sharedResourceRwLock;


    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();
        var options = new ParallelOptions() { MaxDegreeOfParallelism = WritersThreads };

        var writeTask = new Task(() =>
        {
            Parallel.For(0, WritersThreads, options, i =>
            {
                _sharedResource.Write($"Data {i}");

                Thread.Sleep(10);
            });
        });

        var readTask = new Task(() =>
        {
            Parallel.For(0, ReadersThreads, new ParallelOptions() { MaxDegreeOfParallelism = ReadersThreads }, (i) =>
            {
                _sharedResource.Read();
                Thread.Sleep(10);
            });
        });

        writeTask.Start();
        readTask.Start();
        Task.WaitAll(writeTask, readTask);
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", _sharedResource.Read());
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResourceRwLock = new SharedResourceRwLock();

        var writeTask = new Task(() =>
        {
            Parallel.For(0, WritersThreads, new ParallelOptions() { MaxDegreeOfParallelism = WritersThreads }, i =>
            {
                _sharedResourceRwLock.Write($"Data {i}");

                Thread.Sleep(10);
            });
        });

        var readTask = new Task(() =>
        {
            Parallel.For(0, ReadersThreads, new ParallelOptions() { MaxDegreeOfParallelism = ReadersThreads }, (i) =>
            {
                _sharedResourceRwLock.Read();
                Thread.Sleep(10);
            });
        });

        writeTask.Start();
        readTask.Start();
        Task.WaitAll(writeTask, readTask);
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", _sharedResourceRwLock.Read());
    }
}