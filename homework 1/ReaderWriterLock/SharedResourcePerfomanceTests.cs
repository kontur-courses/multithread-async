using System;
using System.Diagnostics;
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

    private readonly IdenticalThreadsStarter _threadsStarter = new();

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
        var stopwatch = Stopwatch.StartNew();

        var readCountdown = _threadsStarter.Start(_ => SimulateRead(), ReadersThreads);
        var writeCountdown = _threadsStarter.Start(_ => SimulateWrite(), WritersThreads);

        readCountdown.Wait();
        writeCountdown.Wait();

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    private void SimulateRead()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.ComputeFactorial(FactorialNumber);
            _sharedResource.Read();
        }
    }

    private void SimulateWrite()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.ComputeFactorial(FactorialNumber);
            _sharedResource.Write("something");
        }
    }
}