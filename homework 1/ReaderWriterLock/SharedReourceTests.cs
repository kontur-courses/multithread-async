using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int Repeat = 5;
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase _sharedResource;
    
    [Test]
    [Repeat(Repeat)]
    public void TestConcurrentReadWrite()
    {
        _sharedResource = new SharedResourceLock();
        TestConcurrent();
    }
    
    [Test]
    [Repeat(Repeat)]
    public void TestConcurrentReadWriteRwLock()
    {
        _sharedResource = new SharedResourceRwLock();
        TestConcurrent();
    }

    private void TestConcurrent()
    {
        var writersThreads = new Thread[WritersThreads];
        var readersThreads = new Thread[ReadersThreads];
        var textLenght = 0;

        for (var i = 0; i < WritersThreads; i++) 
        {
            var text = $"Data {i} ";
            writersThreads[i] = new Thread(() => _sharedResource.Write(text));
            textLenght += text.Length;
        }

        Enumerable.Range(0, ReadersThreads)
            .ForEach(i => readersThreads[i] = new Thread(() => _sharedResource.Read()));
        
        writersThreads.ForEach(x => x.Start());
        readersThreads.ForEach(x => x.Start());
        writersThreads.ForEach(x => x.Join());
        readersThreads.ForEach(x => x.Join());
        
        Assert.That(_sharedResource.Read(), Does.Contain($"Data {WritersThreads-1} "));
        Assert.That(_sharedResource.Read().Length, Is.EqualTo(textLenght));
    }
}