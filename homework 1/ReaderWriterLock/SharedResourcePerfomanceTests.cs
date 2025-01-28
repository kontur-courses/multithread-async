using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 60; // Большое число для вычисления факториала

    [Test]
    public void TestLockPerformance()
    {
        var lockTime = MeasurePerformance(new SharedResourceLock());
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        var rwLockTime = MeasurePerformance(new SharedResourceRwLock());
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

        var readers = Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    sharedResource.Read();
                    sharedResource.ComputeFactorial(FactorialNumber);
                }
            }))
            .ToArray();
        
        var writers = Enumerable
            .Range(0, WritersThreads)
            .Select(i => new Thread(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    sharedResource.Write($"Data {i}");
                    sharedResource.ComputeFactorial(FactorialNumber);
                }
            }))
            .ToArray();

        var sw = new Stopwatch();
        sw.Start();
        writers.ForEach(t => t.Start());
        readers.ForEach(t => t.Start());
        writers.ForEach(t => t.Join());
        readers.ForEach(t => t.Join());
        sw.Stop();
    
        return sw.ElapsedMilliseconds;  
    }
}