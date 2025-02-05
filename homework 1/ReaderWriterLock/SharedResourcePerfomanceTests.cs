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

        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var stopwatch = new Stopwatch();
        var countdown = new CountdownEvent(WritersThreads + ReadersThreads);

        stopwatch.Start();

        for (var i = 0; i < WritersThreads; i++)
        {
            Task.Run(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    _sharedResource.Write($"Data {j}");
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }

                countdown.Signal();
            });
        }

        for (var i = 0; i < ReadersThreads; i++)
        {
            Task.Run(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    _sharedResource.Read();
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }

                countdown.Signal();
            });
        }

        countdown.Wait();
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }
}