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
    private SharedResourceBase _sharedResource;

    private void RunThreads()
    {
        var countdown = new CountdownEvent(ReadersThreads + WritersThreads);
        var startEvent = new ManualResetEventSlim();
        
        for (var i = 0; i < WritersThreads; i++)
        {
            var number = i;
            var thread = new Thread(() =>
            {
                startEvent.Wait();
                _sharedResource.Write($"Data {number}");
                countdown.Signal();
            });
            thread.Start();
        }

        for (var i = 0; i < ReadersThreads; i++)
        {
            var thread = new Thread(() =>
            {
                startEvent.Wait();
                var result = _sharedResource.Read();
                Assert.That(result, Is.Ordered.Ascending);
                result.ForEach(str =>
                    ClassicAssert.IsTrue(str.Split(' ')[0] == "Data" && int.TryParse(str.Split(' ')[1], out _)));
                countdown.Signal();
            });
            thread.Start();
        }
        
        startEvent.Set();
        countdown.Wait();
    }

    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();
        
        RunThreads();
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read().Last());
        ClassicAssert.AreEqual(_sharedResource.Count, WritersThreads);
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();
        
        RunThreads();
        
        ClassicAssert.AreEqual($"Data {WritersThreads-1}", _sharedResource.Read().Last());
        ClassicAssert.AreEqual(_sharedResource.Count, WritersThreads);
    }
}