using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private const int NumberOfIterations = 10000;
    private SharedResourceBase _sharedResource;
    private CountdownEvent countdown;
    private string[] valuesForWrite;

    [TestCase(100)]
    public void TestConcurrentReadWrite(int iterations)
    {
        Enumerable.Range(0, iterations).ForEach((_) =>
        {
            _sharedResource = new SharedResourceLock();
            TestOneAction();
        });
    }

    [TestCase(100)]
    public void TestConcurrentReadWriteRwLock(int iterations)
    {
        Enumerable.Range(0, iterations).ForEach((_) =>
        {
            _sharedResource = new SharedResourceRwLock();
            TestOneAction();
        });
    }

    private void TestOneAction()
    {
        countdown = new CountdownEvent(WritersThreads + ReadersThreads);
        var rnd = new Random();
        valuesForWrite = new string[WritersThreads]
            .Select(_ => rnd.GetRandomString(10, 100))
            .ToArray();
        var writers = new int[WritersThreads].Select((_, i) => i);
        var readers = new int[ReadersThreads];
        var tasks = writers.Concat(readers).ToArray();
        new Random().Shuffle(tasks);

        foreach (var i in tasks)
        {
            if (i > 0)
            {
                ThreadPool.QueueUserWorkItem((cb) =>
                {
                    var value = valuesForWrite[i];
                    _sharedResource.Write(value, i);
                    countdown.Signal();
                });
            }
            else
            {
                ThreadPool.QueueUserWorkItem((cb) =>
                {
                    _sharedResource.Read();
                    countdown.Signal();
                });
            }
        }
        countdown.Wait();
        ClassicAssert.AreEqual(valuesForWrite[_sharedResource.LastWriterThreadIndex], _sharedResource.Read());
    }
}