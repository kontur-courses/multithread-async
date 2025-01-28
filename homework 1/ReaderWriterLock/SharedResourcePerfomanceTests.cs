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
    private const int NumberOfIterations = 100;
    private const int FactorialNumber = 60; // Большое число для вычисления факториала

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
        var readers = CreateThreads(() => RepeatAction(NumberOfIterations, Read), ReadersThreads);
        var writers = CreateThreads(() => RepeatAction(NumberOfIterations, Write), WritersThreads);
        var stopwatch = Stopwatch.StartNew();
        var threads = readers.Concat(writers)
            .ToArray();
        
        stopwatch.Start();
        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());
        stopwatch.Stop();
        
        return stopwatch.ElapsedMilliseconds;
    }

    private IEnumerable<Thread> CreateThreads(Action action, int threadCount)
    {
        return Enumerable.Range(0, threadCount).Select(_ => new Thread(new ThreadStart(action)));
    }

    private void RepeatAction(int iterations, Action action)
    {
        for (var i = 0; i < iterations; i++)
            action();
    }
    
    private void Read()
    {
        _sharedResource.Read();
        _sharedResource.ComputeFactorial(FactorialNumber);
    }

    private void Write()
    {
        _sharedResource.Write("Данные ");
        _sharedResource.ComputeFactorial(FactorialNumber);
    }
}