using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private SharedResourceBase sharedResource;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 60;

    [Test]
    public void TestLockPerformance()
    {
        sharedResource = new SharedResourceLock();
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");
        
        rwLockTime.Should().BeLessThan(lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var threads = CreateThreads().ToArray();
        var sw = Stopwatch.StartNew();
        
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
        sw.Stop();
        
        return sw.ElapsedMilliseconds;
    }

    private IEnumerable<Thread> CreateThreads()
    {
        var writingThreads = Enumerable
            .Range(0, WritersThreads)
            .Select(_ => new Thread(WriteAction));
        var readingThreads = Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(ReadAction));
        
        return writingThreads.Concat(readingThreads);
    }

    private void WriteAction()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            sharedResource.Write($"Data {i}");
            sharedResource.ComputeFactorial(FactorialNumber);
        }
    }

    private void ReadAction()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            sharedResource.Read();
            sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
    
}