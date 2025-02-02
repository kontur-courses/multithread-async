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
        var threads = Enumerable.Range(0, WritersThreads)
            .Select(i => new Thread(() => _sharedResource.Write($"Data {i}")))
            .Union(Enumerable.Range(0, ReadersThreads)
                .Select(_ => new Thread(() => _sharedResource.Read())))
            .ToArray();
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());    

        return ($"Data {WritersThreads - 1}".Equals(_sharedResource.Read()));
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