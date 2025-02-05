using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private SharedResourceBase _sharedResource;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 100;
    private const int FactorialNumber = 1000;

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock();
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        Assert.That(rwLockTime, Is.LessThan(lockTime), "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var stopwatch = Stopwatch.StartNew();
        var writersThreads = new Thread[WritersThreads];
        var readersThreads = new Thread[ReadersThreads];

        for (var i = 0; i < WritersThreads; i++)
        {
            writersThreads[i] = new Thread(() =>
            {
                for (var n = 0; n < NumberOfIterations; n++)
                {
                    _sharedResource.Write($"Data {n} ");
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            });
        }

        for (var i = 0; i < ReadersThreads; i++)
        {
            readersThreads[i] = new Thread(() =>
            {
                for (var n = 0; n < NumberOfIterations; n++)
                {
                    _sharedResource.Read();
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            });
        }

        writersThreads.ForEach(x => x.Start());
        readersThreads.ForEach(x => x.Start());
        writersThreads.ForEach(x => x.Join());
        readersThreads.ForEach(x => x.Join());

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}