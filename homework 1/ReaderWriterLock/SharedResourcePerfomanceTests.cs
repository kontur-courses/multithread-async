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
        var threads = new List<Thread>();
        threads.AddRange(
            CreateThreads(ReadersThreads, NumberOfIterations, () => _sharedResource.Read()));
        threads.AddRange(
            CreateThreads(WritersThreads, NumberOfIterations, () => _sharedResource.Write("Data")));

        var stopwatch = Stopwatch.StartNew();
        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private List<Thread> CreateThreads(int threadCount, int numberOfIterations, Action action)
    {
        var threads = new List<Thread>(threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            var thread = new Thread(_ =>
            {
                for (int j = 0; j < numberOfIterations; j++)
                {
                    action.Invoke();
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            });

            threads.Add(thread);
        }

        return threads;
    }
}