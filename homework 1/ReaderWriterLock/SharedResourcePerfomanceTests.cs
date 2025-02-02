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
    private const int NumberOfIterations = 1000;
    private const int FactorialNumber = 200;

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock();
        long lockTime = MeasurePerformance();

        _sharedResource = new SharedResourceRwLock();
        long rwLockTime = MeasurePerformance();

        Console.WriteLine($"Lock time: {lockTime} ms");
        Console.WriteLine($"ReaderWriterLock time: {rwLockTime} ms");

        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var threads = new List<Thread>();

        Enumerable.Range(0, WritersThreads).ForEach(_ => 
        {
            var t = new Thread(() => {
                for (int i = 0; i < NumberOfIterations; i++)
                {
                    _sharedResource.Write($"Data {i}");
                }
            });
            threads.Add(t);
            t.Start();
        });

        Enumerable.Range(0, ReadersThreads).ForEach(_ => 
        {
            var t = new Thread(() => {
                for (int i = 0; i < NumberOfIterations; i++)
                {
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            });
            threads.Add(t);
            t.Start();
        });

        var sw = Stopwatch.StartNew();

        threads.ForEach(t => t.Join());
    
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }
}