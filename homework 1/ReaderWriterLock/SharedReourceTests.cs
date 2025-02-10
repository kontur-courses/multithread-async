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

    [Test]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();

        var writeThreads = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(_ => _sharedResource.Write($"Data {i}")))
            .ToArray();
        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(_ => _sharedResource.Read()))
            .ToArray();

        writeThreads.ForEach(t => t.Start());
        readThreads.ForEach(t => t.Start());

        writeThreads.ForEach(t => t.Join());
        readThreads.ForEach(t => t.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", _sharedResource.Read());
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();

        var writeThreads = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(_ => _sharedResource.Write($"Data {i}")))
            .ToArray();
        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(_ => _sharedResource.Read()))
            .ToArray();

        writeThreads.ForEach(t => t.Start());
        readThreads.ForEach(t => t.Start());

        writeThreads.ForEach(t => t.Join());
        readThreads.ForEach(t => t.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", _sharedResource.Read());
    }
}