using System;
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
    private const int FactorialNumber = 150; // Большое число для вычисления факториала

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock();
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
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
        var writeThreads = Enumerable.Range(0, WritersThreads).Select(_ => new Thread(WriteData));
        var readThreads = Enumerable.Range(0, ReadersThreads).Select(_ => new Thread(ReadData));
        var threads = writeThreads.Concat(readThreads).ToList();
        
        var sw = Stopwatch.StartNew();
        
        threads.AsParallel().ForAll(t => t.Start());
        threads.AsParallel().ForAll(t => t.Join());

        sw.Stop();
        
        return sw.ElapsedMilliseconds;
    }

    private void WriteData()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Write($"Data {i}. ");
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }

    private void ReadData()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Read();
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
}