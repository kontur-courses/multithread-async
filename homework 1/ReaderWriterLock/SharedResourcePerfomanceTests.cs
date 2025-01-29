using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private SharedResourceBase sharedResource;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 60; // Большое число для вычисления факториала

    [Test]
    public void TestLockPerformance()
    {
        sharedResource = new SharedResourceLock();
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");
        
        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task>();
        var random = new Random();
        
        for (int i = 0; i < WritersThreads; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < NumberOfIterations; j++)
                {
                    sharedResource.Write($"Data {random.Next()}");
                    sharedResource.ComputeFactorial(FactorialNumber);
                }
            }));
        }
        
        for (int i = 0; i < ReadersThreads; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < NumberOfIterations; j++)
                {
                    sharedResource.Read();
                    sharedResource.ComputeFactorial(FactorialNumber);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}