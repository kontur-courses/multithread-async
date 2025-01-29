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
    private const int ReadersThreads = 100;
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
        var writers = CreateThreads(WritersThreads, () => sharedResource.Write("bla-bla"));
        var readers = CreateThreads(ReadersThreads, () => sharedResource.Read());

        var stopwatch = Stopwatch.StartNew();
        writers.ForEach(writer => writer.Start());
        readers.ForEach(reader => reader.Start());

        writers.ForEach(writer => writer.Join());
        readers.ForEach(reader => reader.Join());
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }

    private List<Thread> CreateThreads(int count, Action action) =>
        Enumerable
            .Range(0, count)
            .Select(_ => new Thread(() => SimulatePayload(action)))
            .ToList();

    private void SimulatePayload(Action action) =>
        Enumerable.Range(0, NumberOfIterations).ForEach(_ =>
        {
            action();
            sharedResource.ComputeFactorial(FactorialNumber);
        });
}