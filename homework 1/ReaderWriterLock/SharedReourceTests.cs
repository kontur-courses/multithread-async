using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using FluentAssertions;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreads = 100;
    private const int ReadersThreads = 1000;
    private SharedResourceBase _sharedResource;

    private bool SharedResourcesContainsLastThreadData(SharedResourceBase sharedResource)
    {
        _sharedResource = sharedResource;
        var writeThreads = new List<Thread>();
        var readThreads = new List<Thread>();
        
        for (var i = 0; i < WritersThreads; i++)
        {
            var thread = new Thread(() => _sharedResource.Write($"Data {i}"));
            writeThreads.Add(thread);
            thread.Start();
        }
        
        for (var i = 0; i < ReadersThreads; i++)
        {
            var thread = new Thread(() => _sharedResource.Read());
            readThreads.Add(thread);
            thread.Start();
        }
        
        writeThreads.ForEach(t => t.Join());
        readThreads.ForEach(t => t.Join());

        return ($"Data {WritersThreads}".Equals(_sharedResource.Read()));
    }

    [Test]
    public void TestConcurrentReadWrite()
    {
        SharedResourcesContainsLastThreadData(new SharedResourceLock()).Should().BeTrue();
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        SharedResourcesContainsLastThreadData(new SharedResourceRwLock()).Should().BeTrue();
    }
}