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
    private SharedResourceBase _sharedResource;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 1000;
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
        var threads = Enumerable
            .Range(0, ReadersThreads)
            .Select(_ => new Thread(Read))
            .Concat(Enumerable.Range(0, WritersThreads).Select(_ => new Thread(Write)))
            .ToArray();

        var sw = Stopwatch.StartNew();
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
        sw.Stop();

        return sw.ElapsedMilliseconds;
    }


    private void Write()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Write(i.ToString());
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }

    private void Read()
    {
        for (var i = 0; i < NumberOfIterations; i++)
        {
            _sharedResource.Read();
            _sharedResource.ComputeFactorial(FactorialNumber);
        }
    }
}