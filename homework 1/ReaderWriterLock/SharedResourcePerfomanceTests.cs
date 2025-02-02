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
        var timer = Stopwatch.StartNew();

        var writeThreads = CreateThreads(WritersThreads, RepeatWriting);
        var readThreads = CreateThreads(ReadersThreads, RepeatReading);
        writeThreads.ForEach(t => t.Start());
        readThreads.ForEach(t => t.Start());
        writeThreads.ForEach(t => t.Join());
        readThreads.ForEach(t => t.Join());
        
        timer.Stop();
        return timer.ElapsedMilliseconds;
    }

    private static Thread[] CreateThreads(int count, Action action)
    {
        return Enumerable.Range(0,count)
            .Select(_ => new Thread(_=> action.Invoke()))
            .ToArray();
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