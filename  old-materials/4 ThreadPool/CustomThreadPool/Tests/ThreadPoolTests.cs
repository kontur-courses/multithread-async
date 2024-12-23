using System;
using System.Diagnostics;
using System.Threading;

namespace CustomThreadPool
{
    public class ThreadPoolTests
    {
        public static void Run<TThreadPool>() where TThreadPool : IThreadPool, new()
        {
            Run(() => new TThreadPool());
        }
        
        public static void Run(Func<IThreadPool> threadPoolFactory)
        {
            var name = threadPoolFactory().GetType().Name.Replace("ThreadPool", "", StringComparison.OrdinalIgnoreCase);

            Console.WriteLine($"----------======={name} ThreadPool tests=======----------");
            
            RunTest(LongCalculations);
            RunTest(ShortCalculations);
            RunTest(ExtremelyShortCalculations);
            RunTest(InnerShortCalculations);
            RunTest(InnerExtremelyShortCalculations);
            
            Console.WriteLine("\n");
            
            void RunTest(Action<IThreadPool> test) => test(threadPoolFactory());
        }
        
        private static void LongCalculations(IThreadPool threadPool)
        {
            Console.Write("LongCalculations test: ");
            var timer = Stopwatch.StartNew();
            long enqueueMs;

            const int actionsCount = 1 * 1000;

            using(var cev = new CountdownEvent(actionsCount))
            {
                Action sumAction = () =>
                {
                    cev.Signal();
                    Thread.SpinWait(1000 * 1000);
                };
                for(int i = 0; i < actionsCount; i++)
                {
                    threadPool.EnqueueAction(sumAction);
                }
                enqueueMs = timer.ElapsedMilliseconds;
                cev.Wait();
            }
            timer.Stop();
            Console.WriteLine($" total {timer.ElapsedMilliseconds} ms, enqueue {enqueueMs} ms [tasks processed ~{threadPool.GetTasksProcessedCount()}]");
        }

        private static void ShortCalculations(IThreadPool threadPool)
        {
            Console.Write("ShortCalculations test: ");
            var timer = Stopwatch.StartNew();
            long enqueueMs;

            const int actionsCount = 1 * 1000 * 1000;

            using(var cev = new CountdownEvent(actionsCount))
            {
                Action sumAction = () =>
                {
                    cev.Signal();
                    Thread.SpinWait(1000);
                };
                for(var i = 0; i < actionsCount; i++)
                { 
                    threadPool.EnqueueAction(sumAction);
                }
                enqueueMs = timer.ElapsedMilliseconds;
                cev.Wait();
            }
            timer.Stop();
            Console.WriteLine($" total {timer.ElapsedMilliseconds} ms, enqueue {enqueueMs} ms [tasks processed ~{threadPool.GetTasksProcessedCount()}]");
        }

        private static void ExtremelyShortCalculations(IThreadPool threadPool)
        {
            Console.Write("ExtremelyShortCalculations test: ");
            var timer = Stopwatch.StartNew();
            long enqueueMs;

            const int actionsCount = 1 * 1000 * 1000;

            using(var cev = new CountdownEvent(actionsCount))
            {
                Action sumAction = () =>
                {
                    cev.Signal();
                };
                for(int i = 0; i < actionsCount; i++)
                {
                    threadPool.EnqueueAction(sumAction);
                }
                enqueueMs = timer.ElapsedMilliseconds;
                cev.Wait();
            }
            timer.Stop();
            Console.WriteLine($" total {timer.ElapsedMilliseconds} ms, enqueue {enqueueMs} ms [tasks processed ~{threadPool.GetTasksProcessedCount()}]");
        }

        private static void InnerShortCalculations(IThreadPool threadPool)
        {
            Console.Write("InnerCalculations test: ");
            var timer = Stopwatch.StartNew();
            long enqueueMs;

            const int actionsCount = 1 * 1000;
            const int subactionsCount = 1 * 1000;

            using(CountdownEvent outerEvent = new CountdownEvent(actionsCount))
            using(CountdownEvent innerEvent = new CountdownEvent(actionsCount * subactionsCount))
            {
                Action innerAction = () =>
                {
                    innerEvent.Signal();
                    Thread.SpinWait(1000);
                };
                Action outerAction = () =>
                {
                    for(int i = 0; i < subactionsCount; i++)
                    {
                        threadPool.EnqueueAction(innerAction);
                    }
                    outerEvent.Signal();
                };

                for(int i = 0; i < actionsCount; i++)
                {
                    threadPool.EnqueueAction(outerAction);
                }

                outerEvent.Wait();
                enqueueMs = timer.ElapsedMilliseconds;
                innerEvent.Wait();
            }
            timer.Stop();
            Console.WriteLine($" total {timer.ElapsedMilliseconds} ms, enqueue {enqueueMs} ms [tasks processed ~{threadPool.GetTasksProcessedCount()}]");
        }

        private static void InnerExtremelyShortCalculations(IThreadPool threadPool)
        {
            Console.Write("InnerExtremelyShortCalculations test: ");
            var timer = Stopwatch.StartNew();
            long enqueueMs;

            const int actionsCount = 1 * 1000;
            const int subactionsCount = 1 * 1000;

            using(CountdownEvent outerEvent = new CountdownEvent(actionsCount))
            using(CountdownEvent innerEvent = new CountdownEvent(actionsCount * subactionsCount))
            {
                Action innerAction = () =>
                {
                    innerEvent.Signal();
                };
                Action outerAction = () =>
                {
                    for(int i = 0; i < subactionsCount; i++)
                    {
                        threadPool.EnqueueAction(innerAction);
                    }
                    outerEvent.Signal();
                };

                for(int i = 0; i < actionsCount; i++)
                {
                    threadPool.EnqueueAction(outerAction);
                }

                outerEvent.Wait();
                enqueueMs = timer.ElapsedMilliseconds;
                innerEvent.Wait();
            }
            timer.Stop();
            Console.WriteLine($" total {timer.ElapsedMilliseconds} ms, enqueue {enqueueMs} ms [tasks processed ~{threadPool.GetTasksProcessedCount()}]");
        }
    }
}