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
    private const int FactorialNumber = 70; // Большое число для вычисления факториала

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

    // Нужно реализовать тест производительности.
    // В многопоточном режиме нужно запустить:
    // - Чтение общего ресурса в количестве ReadersThreads читающих потоков
    // - Запись значений в количестве WritersThreads записывающих потоков
    // - В вызовах читателей и писателей обязательно нужно вызывать подсчет факториала для симуляции полезной нагрузки
    private long MeasurePerformance()
    {
        var writers = CreateThreads(WritersThreads, () => _sharedResource.Write("")).ToList();
        var readers = CreateThreads(ReadersThreads, () => _sharedResource.Read()).ToList();

        var allThreads = writers.Concat(readers).ToList();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        allThreads.ForEach(thread => thread.Start());
        allThreads.ForEach(thread => thread.Join());
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private IEnumerable<Thread> CreateThreads(int numberOfThreads, Action action) =>
        Enumerable.Range(0, numberOfThreads).Select(x => new Thread(() =>
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                action.Invoke();
                _sharedResource.ComputeFactorial(FactorialNumber);
            }
        }));
}