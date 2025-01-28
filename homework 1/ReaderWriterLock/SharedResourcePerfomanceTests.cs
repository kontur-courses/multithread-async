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
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 1000;
    private const int FactorialNumber = 60; // Большое число для вычисления факториала

    [Test]
    public void TestLockPerformance()
    {
        var lockTime = MeasurePerformance(new SharedResourceLock());
        var rwLockTime = MeasurePerformance(new SharedResourceRwLock());
        
        Console.WriteLine($"Lock time taken: {lockTime} ms");
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        // Проверка, что время выполнения с ReaderWriterLock меньше, чем с Lock
        Assert.That(rwLockTime, Is.LessThan(lockTime), "ReaderWriterLock should be faster than Lock");
    }

    private static long MeasurePerformance(SharedResourceBase sharedResource)
    {
        // Нужно реализовать тест производительности.
        // В многопоточном режиме нужно запустить:
        // - Чтение общего ресурса в количестве ReadersThreads читающих потоков
        // - Запись значений в количестве WritersThreads записывающих потоков
        // - В вызовах читателей и писателей обязательно нужно вызывать подсчет факториала для симуляции полезной нагрузки
        
        var writers = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(() =>
            {
                for (var _ = 0; _ < NumberOfIterations; _++)
                {
                    sharedResource.Write($"Data: {i}");
                    sharedResource.ComputeFactorial(FactorialNumber);
                }
            })).ToList();

        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() =>
            {
                for (var __ = 0; __ < NumberOfIterations; __++)
                {
                    sharedResource.Read();
                    sharedResource.ComputeFactorial(FactorialNumber);
                }
            })).ToList();
        
        var sw = Stopwatch.StartNew();
        writers.ForEach(w => w.Start());
        readers.ForEach(r => r.Start());
        
        writers.ForEach(w => w.Join());
        readers.ForEach(r => r.Join());
        sw.Stop();
        
        return sw.ElapsedMilliseconds;
    }
}