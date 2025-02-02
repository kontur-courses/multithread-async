using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        var timer = Stopwatch.StartNew();
        var writeThreads = new List<Thread>();
        var readThreads = new List<Thread>();

        for (var i = 0; i < WritersThreads; i++)
        {
            var thread = new Thread(RepeatWriting);
            writeThreads.Add(thread);
            thread.Start();
        }

        for (var i = 0; i < ReadersThreads; i++)
        {
            var thread = new Thread(RepeatReading);
            readThreads.Add(thread);
            thread.Start();
        }
        
        writeThreads.ForEach(t => t.Join());
        readThreads.ForEach(t => t.Join());
        
        timer.Stop();
        return timer.ElapsedMilliseconds;
    }

    private void RepeatWriting()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Write($"Data {i}");
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }

    private void RepeatReading()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Read();
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
}