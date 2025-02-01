using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 500;
    private static readonly ManualResetEvent Event = new(false);
    private SharedResourceBase _sharedResource;

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock();
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        Event.Reset();
        _sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        rwLockTime.Should().BeLessThan(lockTime);
    }

    private long MeasurePerformance()
    {
        var writers = Enumerable.Range(0, WritersThreads).Select(_ => new Thread(WriteData));
        var readers = Enumerable.Range(0, ReadersThreads).Select(_ => new Thread(ReadData));
        var threads = writers.Concat(readers).ToArray();
        var sw = Stopwatch.StartNew();

        threads.ForEach(t => t.Start());
        Event.Set();
        threads.ForEach(t => t.Join());

        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    private void WriteData()
    {
        Event.WaitOne();
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Write("Some data. ");
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }

    private void ReadData()
    {
        Event.WaitOne();
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Read();
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
}