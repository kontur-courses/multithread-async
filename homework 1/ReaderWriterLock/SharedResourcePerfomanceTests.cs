using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
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
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        // Проверка, что время выполнения с ReaderWriterLock меньше, чем с Lock
        rwLockTime.Should().BeLessThan(lockTime, "because rwlock time is less than the lock time");
    }

    private long MeasurePerformance()
    {
        var readerThreads = Enumerable.Range(0, ReadersThreads).Select(_ => new Thread(Read));
        var writerThreads = Enumerable.Range(0, WritersThreads).Select(_ => new Thread(Write));

        var threads = writerThreads.Concat(readerThreads).ToArray();

        var stopwatch = Stopwatch.StartNew();
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private void Read()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Read();
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }

    private void Write()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Write("Data");
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
}