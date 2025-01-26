using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        var threads = new List<Thread>();
        var readThreads =
            CreateThreadsWithSimulatesPayload(ReadersThreads, NumberOfIterations, () => sharedResource.Read());
        var writeThreads =
            CreateThreadsWithSimulatesPayload(WritersThreads, NumberOfIterations, () => sharedResource.Write("Data"));
        threads.AddRange(readThreads);
        threads.AddRange(writeThreads);

        var stopwatch = Stopwatch.StartNew();
        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private List<Thread> CreateThreadsWithSimulatesPayload(int threadCount, int numberOfIterations,
        Action threadAction)
    {
        var threads = new List<Thread>(threadCount);
        for (var i = 0; i < threadCount; i++)
        {
            var thread = new Thread(_ =>
            {
                for (var j = 0; j < numberOfIterations; j++)
                {
                    threadAction.Invoke();
                    sharedResource.ComputeFactorial(FactorialNumber);
                }
            });
            threads.Add(thread);
        }

        return threads;
    }
}