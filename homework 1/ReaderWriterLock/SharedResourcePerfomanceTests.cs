using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private ISharedResource _sharedResource;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 60; // Большое число для вычисления факториала

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResource();
        long lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRWLock();
        long rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        // Проверка, что время выполнения с ReaderWriterLock меньше, чем с Lock
        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        // Реализацию заменить на throw new NotImplementedException(). Нужно будет им самим реализовать тест производительности
        var threads = new List<Thread>();
        
        for (var i = 0; i < ReadersThreads; i++)
        {
            threads.Add(new Thread(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    var _ = _sharedResource.Read();
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            }));
        }
        for (var i = 0; i < WritersThreads; i++)
        {
            threads.Add(new Thread(() =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    _sharedResource.Write("Big writed data" + j*j*j);
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            }));
        }
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}