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
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");
        
        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var allTreads = CreateTreads().ToArray();
        var stopwatch = Stopwatch.StartNew();
        
        allTreads.ForEach(thread => thread.Start());
        allTreads.ForEach(thread => thread.Join());
        
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
    
    private IEnumerable<Thread> CreateTreads()
    {
        var writingThreads = Enumerable
            .Range(0, WritersThreads)
            .Select(_ => new Thread(WritingAction));
        var readingThreads = Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(ReadingAction));
        
        return writingThreads.Concat(readingThreads);
    }

    private void WritingAction()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Write($"Data {i}");
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
    
    private void ReadingAction()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Read();
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
}