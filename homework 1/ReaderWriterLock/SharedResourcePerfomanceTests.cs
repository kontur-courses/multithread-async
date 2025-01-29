using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ReaderWriterLock;

public class SharedResourcePerformanceTests
{
    private SharedResourceBase _sharedResource;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 60;

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock();
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        rwLockTime.Should().BeLessThan(lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var stopwatch = Stopwatch.StartNew();

        var writers = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    _sharedResource.Write($"Data {i}");
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            })).ToArray();

        var readers = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    _sharedResource.Read();
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            })).ToArray();

        writers.ForEach(writer => writer.Start());
        readers.ForEach(reader => reader.Start());

        writers.ForEach(writer => writer.Join());
        readers.ForEach(reader => reader.Join());

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}