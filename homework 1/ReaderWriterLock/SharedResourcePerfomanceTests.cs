using System;
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
    private const int FactorialNumber = 1000;
    private ManualResetEvent manualResetEvent;

    [Test]
    public void TestLockPerformance()
    {
        sharedResource = new SharedResourceLock();
        long lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        sharedResource = new SharedResourceRwLock();
        long rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        rwLockTime.Should().BeLessThan(lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var dataToWrite = "Test data";
        manualResetEvent = new ManualResetEvent(false);

        var writers = CreateThreadsWithPayload(
            () => sharedResource.Write(dataToWrite),
            () => sharedResource.ComputeFactorialWrite(FactorialNumber),
            WritersThreads);

        var readers = CreateThreadsWithPayload(
            () => sharedResource.Read(),
            () => sharedResource.ComputeFactorialRead(FactorialNumber),
            ReadersThreads);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        manualResetEvent.Set();
        writers.ForEach(x => x.Join());
        readers.ForEach(x => x.Join());

        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private Thread[] CreateThreadsWithPayload(
        Action sharedResourceAction,
        Action PayLoadAction,
        int threadCount)
    {
        var result = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            result[i] = new Thread(() =>
            {
                manualResetEvent.WaitOne();
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    sharedResourceAction.Invoke();
                    PayLoadAction.Invoke();
                }
            });
            result[i].Start();
        }
        return result;
    }
}