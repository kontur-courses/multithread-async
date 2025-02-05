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

    [Test]
    public void TestConcurrentReadWrite()
    {
        var resource = new SharedResourceLock();

        var writeThreads = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(() => resource.Write($"Data {i}")))
            .ToList();

        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => resource.Read()))
            .ToList();

        var allThreads = writeThreads.Concat(readThreads).ToList();

        allThreads.ForEach(t => t.Start());
        allThreads.ForEach(t => t.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", resource.Read());
    }

    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        var resource = new SharedResourceRwLock();

        var writeThreads = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(() => resource.Write($"Data {i}")))
            .ToList();

        var readThreads = Enumerable.Range(0, ReadersThreads)
            .Select(_ => new Thread(() => resource.Read()))
            .ToList();

        var allThreads = writeThreads.Concat(readThreads).ToList();

        allThreads.ForEach(t => t.Start());
        allThreads.ForEach(t => t.Join());

        ClassicAssert.AreEqual($"Data {WritersThreads - 1}", resource.Read());
    }
}