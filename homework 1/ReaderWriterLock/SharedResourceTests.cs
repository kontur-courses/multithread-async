using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ReaderWriterLock;

[TestFixture]
public class SharedResourceTests
{
    private const int WritersThreadsNumber = 100;
    private const int ReadersThreadsNumber = 1000;

    [Test]
    public void TestConcurrentReadWrite()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной.
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        TestConcurrentWithSharedResource(new SharedResourceLock());
    }
    
    [Test]
    public void TestConcurrentReadWriteRwLock()
    {
        // Реализовать проверку конкурентной записи и чтения, где в конце должны проверить что данные последнего потока записаны
        // Проверка должна быть многопоточной
        // Потоков чтения должно быть ReadersThreads, потоков записи должно быть WritersThreads
        TestConcurrentWithSharedResource(new SharedResourceRwLock());
    }

    private void TestConcurrentWithSharedResource(SharedResourceBase sharedResource)
    {
        var data = Enumerable.Range(1, WritersThreadsNumber)
            .Select(i => $"{i}")
            .ToList();
        var writeThreads = InitializeWriters(sharedResource, data);
        var readThreads = InitializeReaders(sharedResource);
        var threads = writeThreads.Concat(readThreads).ToList();
        
        ExecuteThreadsAsParallel(threads);
        
        sharedResource.Read().Should().BeEquivalentTo($"{data[^1]}");   
    }

    private static IEnumerable<Thread> InitializeWriters(SharedResourceBase sharedResource, IEnumerable<string> data)
        => data.Select(str => new Thread(() => sharedResource.Write(str)));

    private static IEnumerable<Thread> InitializeReaders(SharedResourceBase sharedResource)
        => Enumerable
            .Range(1, ReadersThreadsNumber)
            .Select(_ => new Thread(() => sharedResource.Read()));

    private static void ExecuteThreadsAsParallel(List<Thread> threads)
    {
        threads.AsParallel().ForAll(t => t.Start());
        threads.AsParallel().ForAll(t => t.Join());
    }
    
}