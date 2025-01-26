using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase _sharedResource;

    [Test]
    public void TestConcurrentReadWriteLock()
    {
        var sharedResource = new SharedResourceLock();
        TestConcurrentReadWrite(sharedResource);
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        var sharedResource = new SharedResourceRwLock();
        TestConcurrentReadWrite(sharedResource);
    }

    private void TestConcurrentReadWrite(SharedResourceBase sharedResource)
    {
        var threads = new List<Thread>();
        var countdown = new CountdownEvent(WritersThreads + ReadersThreads);

        for (var i = 0; i < WritersThreads; i++)
        {
            var index = i;
            var thread = new Thread(() =>
            {
                sharedResource.Write($"Data {index}");
                countdown.Signal();
            });
            threads.Add(thread);
            thread.Start();
        }

        for (var i = 0; i < ReadersThreads; i++)
        {
            var thread = new Thread(() =>
            {
                var data = sharedResource.Read();
                countdown.Signal();
            });
            threads.Add(thread);
            thread.Start();
        }

        countdown.Wait();

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", sharedResource.Read());
    }
}
