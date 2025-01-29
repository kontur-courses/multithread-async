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

    //Сделал немного больше, чтобы разница была заметнее
    private const int FactorialNumber = 100; // Большое число для вычисления факториала

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

        var stopwatch = new Stopwatch();

        var threads = CreateWriteThreads().Concat(CreateReadThreads()).ToArray();

        stopwatch.Start();

        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());

        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private IEnumerable<Thread> CreateWriteThreads()
    {
        return Enumerable.Range(0, WritersThreads)
            .Select(_ => new Thread(() =>
            {
                for (int i = 0; i < NumberOfIterations; i++)
                {
                    _sharedResource.Write($"Data {i}");
                    _sharedResource.ComputeFactorialWrite(FactorialNumber);
                }
            }));
    }

    private IEnumerable<Thread> CreateReadThreads()
    {
        return Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() =>
            {
                for (int i = 0; i < NumberOfIterations; i++)
                {
                    _sharedResource.Read();
                    _sharedResource.ComputeFactorialRead(FactorialNumber);
                }
            }));
    }
}