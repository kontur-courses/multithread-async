using System;
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
    private const int NumberOfIterations = 10000;
    private const int FactorialNumber = 60; // Большое число для вычисления факториала

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock() { FactorialNumberForLoadImitation = FactorialNumber};
        long lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock() { FactorialNumberForLoadImitation = FactorialNumber };
        long rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        // Проверка, что время выполнения с ReaderWriterLock меньше, чем с Lock
        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    CountdownEvent countdown = new CountdownEvent(WritersThreads + ReadersThreads);

    private long MeasurePerformance()
    {
        var timer = new Stopwatch();
        var resultTime = 0L;
        
        var rnd = new Random();
        var valueForWrite = rnd.GetRandomString(1, 5);

        Enumerable.Range(0, NumberOfIterations).ForEach(_ =>
        {
            countdown = new CountdownEvent(WritersThreads + ReadersThreads);
            var writers = new int[WritersThreads];
            var readers = new int[ReadersThreads].Select(x => 1);

            var tasks = writers.Concat(readers).ToArray();

            timer = Stopwatch.StartNew();
            foreach (var task in tasks)
            {
                if (task == 0)
                    ThreadPool.QueueUserWorkItem((cb) => WriteWithFactorial(valueForWrite));
                else
                    ThreadPool.QueueUserWorkItem((cb) => ReadWithFactorial());
            }

            countdown.Wait();
            timer.Stop();
            resultTime += timer.ElapsedMilliseconds;
        });
        return resultTime;
    }

    private void ReadWithFactorial()
    {
        _sharedResource.Read(true);
        countdown.Signal();
    }

    private void WriteWithFactorial(string data)
    {
        _sharedResource.Write(data, 0, true);
        countdown.Signal();
    }
}