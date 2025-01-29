using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourcePerformanceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int FactorialNumber = 200;

    [Test]
    public void TestLockPerformance()
    {
        var lockResource = new SharedResourceLock();
        var rwLockResource = new SharedResourceRwLock();

        long lockTime = MeasurePerformance(lockResource);
        long rwLockTime = MeasurePerformance(rwLockResource);

        Console.WriteLine($"Lock time: {lockTime} ms");
        Console.WriteLine($"ReaderWriterLock time: {rwLockTime} ms");

        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance(SharedResourceBase resource)
    {
        var sw = Stopwatch.StartNew();
        Parallel.For(0, WritersThreads, i => resource.Write($"Data {i}"));
        Parallel.For(0, ReadersThreads, _ => resource.ComputeFactorial(FactorialNumber));
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }
}