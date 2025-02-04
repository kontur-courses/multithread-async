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
    private const int FactorialNumber = 200;

    [Test]
    public void TestLockPerformance()
    {
        _sharedResource = new SharedResourceLock();
        var lockTime = MeasurePerformance();
        Console.WriteLine($"Lock time taken: {lockTime} ms");

        _sharedResource = new SharedResourceRwLock();
        var rwLockTime = MeasurePerformance();
        Console.WriteLine($"ReaderWriterLock time taken: {rwLockTime} ms");

        ClassicAssert.Less(rwLockTime, lockTime, "ReaderWriterLock should be faster than Lock");
    }

    private long MeasurePerformance()
    {
        var writeThreads = CreateThreads(WritersThreads, () => _sharedResource.Write("data"));
        var readThreads = CreateThreads(ReadersThreads, () => _sharedResource.Read());
        
        var sw = Stopwatch.StartNew();
        
        writeThreads.ForEach(t => t.Start());
        readThreads.ForEach(t => t.Start());
        writeThreads.ForEach(t => t.Join());
        readThreads.ForEach(t => t.Join());
        
        sw.Stop();
        
        return sw.ElapsedMilliseconds;
    }

    private Thread[] CreateThreads(int count, Action action)
    {
        return Enumerable.Range(0, count)
            .Select(_ => new Thread(_ =>
            {
                for (var j = 0; j < NumberOfIterations; j++)
                {
                    action.Invoke();
                    _sharedResource.ComputeFactorial(FactorialNumber);
                }
            }))
            .ToArray();
    }
}