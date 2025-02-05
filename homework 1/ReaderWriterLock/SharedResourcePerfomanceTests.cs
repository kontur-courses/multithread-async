using System;
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
    private const int NumberOfIterations = 100;
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
        var totalTime = 0L;
        for (var i = 0; i < NumberOfIterations; i++)
            totalTime += MeasureOneIteration();
        return totalTime / NumberOfIterations;
    }

    private long MeasureOneIteration()
    {
        var countdown = new CountdownEvent(ReadersThreads + WritersThreads);
        var startEvent = new ManualResetEventSlim();
        
        for (var i = 0; i < WritersThreads; i++)
        {
            var thread = new Thread(() =>
            {
                startEvent.Wait();
                _sharedResource.ComputeFactorialWrite(FactorialNumber);
                countdown.Signal();
            });
            thread.Start();
        }

        for (var i = 0; i < ReadersThreads; i++)
        {
            var thread = new Thread(() =>
            {
                startEvent.Wait();
                _sharedResource.ComputeFactorialRead(FactorialNumber);
                countdown.Signal();
            });
            thread.Start();
        }
        
        var sw = Stopwatch.StartNew();
        startEvent.Set();
        countdown.Wait();
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }
}