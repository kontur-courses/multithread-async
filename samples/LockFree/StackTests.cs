using System;
using System.Collections.Concurrent;
using System.Threading;
using NUnit.Framework;

namespace LockFree
{
    public class LockStackTests : StackTests<SimpleStack<int>> { }
    // public class LockFreeStackTests : StackTests<LockFreeStack<int>> { }
    public class ConcurrentStackWrapperTests : StackTests<ConcurrentStackWrapper<int>> { }

    [TestFixture]
    public abstract class StackTests<T> where T:IStack<int>, new()
    {
        [Test]
        public void SimplePushAndPop()
        {
            var stack = new T();
            stack.Push(1);
            Assert.AreEqual(1, stack.Pop());
        }

        [Test]
        public void NullReferenceExceptionWhenPopFromEmpty()
        {
	        Assert.Throws<NullReferenceException>(() =>
	        {
		        var stack = new T();
		        stack.Pop();
	        });
        }

        [Test]
        public void ParallelPush()
        {
            var stack = new T();
            Exception lastException = null;

            Action<int> pushAction = counter =>
                               {
                                   try
                                   {
                                       for (int i = 0; i < counter; i++)
                                           stack.Push(i);
                                   }
                                   catch (Exception ex)
                                   {
                                       lastException = ex;
                                   }
                               };

            const int count = 100000;
            var pushingThreads = new Thread[Environment.ProcessorCount];
            for (int i = 0; i < pushingThreads.Length; i++)
            {
                pushingThreads[i] = new Thread(() => pushAction(count));
                pushingThreads[i].Start();
            }

            foreach (var thread in pushingThreads)
                thread.Join();

            Assert.IsNull(lastException);
            for (int i = 0; i < count * pushingThreads.Length; i++)
                stack.Pop();
            Assert.Throws<NullReferenceException>(() => stack.Pop());
        }
        
        [Test]
        public void ParallelPop()
        {
            var stack = new T();
            Exception lastException = null;

            Action<int> popAction = counter =>
                                     {
                                         try
                                         {
                                             for (int i = 0; i < counter; i++)
                                                 stack.Pop();
                                         }
                                         catch (Exception ex)
                                         {
                                             lastException = ex;
                                         }
                                     };

            const int count = 100000;
            var poppingThreads = new Thread[Environment.ProcessorCount];

            for (int i = 0; i < count * poppingThreads.Length; i++)
                stack.Push(i);

            for (int i = 0; i < poppingThreads.Length; i++)
            {
                poppingThreads[i] = new Thread(() => popAction(count));
                poppingThreads[i].Start();
            }

            foreach (var thread in poppingThreads)
                thread.Join();

            Assert.IsNull(lastException);
            Assert.Throws<NullReferenceException>(() => stack.Pop());
        }
    }
}