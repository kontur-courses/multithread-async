using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase _sharedResource;

    private readonly IdenticalThreadsStarter _threadsStarter = new();

    [Test]
    public void TestConcurrentReadWriteLock()
    {
        _sharedResource = new SharedResourceLock();
        TestConcurrentReadWrite();
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();
        TestConcurrentReadWrite();
    }

    public void TestConcurrentReadWrite()
    {
        var readCountdown = _threadsStarter.Start(_ => _sharedResource.Read(), ReadersThreads);
        var writeCountdown = _threadsStarter.Start(i => _sharedResource.Write($"Data {i}"), WritersThreads);

        readCountdown.Wait();
        writeCountdown.Wait();

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", _sharedResource.Read());
    }
}