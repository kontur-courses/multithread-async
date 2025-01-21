using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace LockFree
{
    public class SimpleQueueTests : QueueTests<SimpleQueue<int>> { }
    // public class LockFreeQueueTests : QueueTests<LockFreeQueue<int>> { }

    [TestFixture]
    public abstract class QueueTests<T> where T : IQueue<int>, new()
    {
        [Test]
        public void SimpleEnqueueAndTryDequeue()
        {
            var queue = new T();
            queue.Enqueue(5);
            int result;
            Assert.IsTrue(queue.TryDequeue(out result));
            Assert.AreEqual(5, result);
        }

        [Test]
        public void CannotDequeueFromEmptyQueue()
        {
            var queue = new T();
            int result;
            Assert.IsFalse(queue.TryDequeue(out result));
        }

        [Test]
        public void Concurrent()
        {
            var queue = new T();
            Exception lastException = null;
            Action<int, int> enqueueAction = (counter, value) =>
                                             {
                                                 try
                                                 {
                                                     for (int i = 0; i < counter; i++)
                                                         queue.Enqueue(value);
                                                 }
                                                 catch (Exception e)
                                                 {
                                                     lastException = e;
                                                     throw;
                                                 }
                                             };
            Action<int, int> dequeueAction = (counter, expectedValue) =>
                                             {
                                                 try
                                                 {
                                                     for (int i = 0; i < counter; i++)
                                                     {
                                                         int result;
                                                         var res = queue.TryDequeue(out result);
                                                         Assert.IsTrue(res);
                                                         Assert.AreEqual(expectedValue, result);
                                                     }
                                                 }
                                                 catch (Exception e)
                                                 {
                                                     lastException = e;
                                                     throw;
                                                 }
                                             };

            int threadsCount = 10;
            const int totalElements = 10 * 1000 * 1000;
            int elementsPerThread = totalElements / threadsCount;

            var enqueueingThreads = new Thread[threadsCount];
            for (int i = 0; i < enqueueingThreads.Length; i++)
                enqueueingThreads[i] = new Thread(() => enqueueAction(elementsPerThread, 5));
            var enqueueTimer = Stopwatch.StartNew();
            for (int i = 0; i < enqueueingThreads.Length; i++)
                enqueueingThreads[i].Start();
            foreach (var thread in enqueueingThreads)
                thread.Join();
            enqueueTimer.Stop();
            Console.WriteLine("Enqueue: {0} ms", enqueueTimer.ElapsedMilliseconds);

            var dequeueTimer = Stopwatch.StartNew();
            var dequeueingThreads = new Thread[threadsCount];
            for (int i = 0; i < dequeueingThreads.Length; i++)
                dequeueingThreads[i] = new Thread(() => dequeueAction(elementsPerThread, 5));
            for (int i = 0; i < dequeueingThreads.Length; i++)
                dequeueingThreads[i].Start();
            foreach (var thread in dequeueingThreads)
                thread.Join();
            dequeueTimer.Stop();
            Console.WriteLine("Dequeue: {0} ms", dequeueTimer.ElapsedMilliseconds);

            Assert.IsNull(lastException);
            Assert.Less(enqueueTimer.Elapsed + dequeueTimer.Elapsed, TimeSpan.FromSeconds(30));
        }
    }
}