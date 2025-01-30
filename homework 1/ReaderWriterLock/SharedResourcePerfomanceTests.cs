using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private SharedResourceBase _sharedResource;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 60; // Большое число для вычисления факториала

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock();
        long lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock();
        long rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        // Проверка, что время выполнения с ReaderWriterLock меньше, чем с Lock
        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        // Нужно реализовать тест производительности.
        // В многопоточном режиме нужно запустить:
        // - Чтение общего ресурса в количестве ReadersThreads читающих потоков
        // - Запись значений в количестве WritersThreads записывающих потоков
        // - В вызовах читателей и писателей обязательно нужно вызывать подсчет факториала для симуляции полезной нагрузки
        var readThreads = CreateListThreads(ReadersThreads, () => _sharedResource.Read());
        var writeThreads = CreateListThreads(WritersThreads, () => _sharedResource.Write("Data"));

        var allThreads = writeThreads.Concat(readThreads);

        var stopwatch = Stopwatch.StartNew();
        allThreads.ForEach(x => x.Start());
        allThreads.ForEach(x => x.Join());
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private List<Thread> CreateListThreads(int threadCount, Action threadAction)
    {
        var threads = new List<Thread>();
        for (var i = 0; i < threadCount; i++)
        {
            var thread = new Thread(_ =>
            {
                for (var j = 0; j < FactorialNumber; j++)
                {
                    threadAction.Invoke();
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            });
            threads.Add(thread);
        }

        return threads;
    }
}