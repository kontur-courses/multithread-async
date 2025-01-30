using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        var lockTime = 0L;
        for (int i = 0; i < NumberOfIterations; i++)
        {
            var sharedResourceLock = new SharedResourceLock();
            var cur = MeasurePerformance(sharedResourceLock, NumberOfIterations);
            Console.WriteLine($"Lock time taken: {cur} ms");
            lockTime += cur;
        }


        var rwLockTime = 0L;
        for (int i = 0; i < NumberOfIterations; i++)
        {
            var sharedResourceRmLock = new SharedResourceRwLock();
            var cur = MeasurePerformance(sharedResourceRmLock, NumberOfIterations);
            Console.WriteLine($"ReaderWriterLock time taken: {cur} ms");
            rwLockTime += cur;
        }

        // Проверка, что время выполнения с ReaderWriterLock меньше, чем с Lock
        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance(SharedResourceBase sharedResourceLock, int numberOfIterations)
    {
        var stopWatch = new Stopwatch();

        var writeTask = new Task(() =>
        {
            Parallel.For(0, WritersThreads, new ParallelOptions() { MaxDegreeOfParallelism = WritersThreads }, i =>
            {
                // - Запись значений в количестве WritersThreads записывающих потоков
                sharedResourceLock.Write($"Data {i}");
                sharedResourceLock.ComputeFactorial(FactorialNumber + numberOfIterations);
            });
        });

        var readTask = new Task(() =>
        {
            // - Чтение общего ресурса в количестве ReadersThreads читающих потоков
            Parallel.For(0, ReadersThreads, new ParallelOptions() { MaxDegreeOfParallelism = ReadersThreads }, (i) =>
            {
                sharedResourceLock.Read();
                sharedResourceLock.ComputeFactorial(FactorialNumber + numberOfIterations);
            });
        });

        stopWatch.Start();
        writeTask.Start();
        readTask.Start();
        Task.WaitAll(writeTask, readTask);
        stopWatch.Stop();

        return stopWatch.ElapsedMilliseconds;
    }
}