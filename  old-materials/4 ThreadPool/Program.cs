using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ThreadPool.Examples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // AutoResetEventDemo();
            // ManualResetEventDemo();
            // PoolingDemo();
            // LiveDemo();
            // PulseWaitDemo();
            // PulseBeforeWait();
            ThreadPoolDemo();
        }

        private static void AutoResetEventDemo()
        {
            var are = new AutoResetEvent(false);

            void ThreadAction()
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is ready");
                are.WaitOne();
                Console.WriteLine($"Hello from thread {Thread.CurrentThread.ManagedThreadId}");
            }

            StartBckThreads(ThreadAction, 10);

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("\nOpen gate for next thread");
                are.Set();
            }
        }

        private static void ManualResetEventDemo()
        {
            var mre = new ManualResetEvent(false);

            void ThreadAction()
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is ready");
                mre.WaitOne();
                Console.WriteLine($"Hello from thread {Thread.CurrentThread.ManagedThreadId}");
            }

            StartBckThreads(ThreadAction, 10);

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("\nOpen gate for next thread");
                mre.Set();
                mre.Reset();
            }
        }
        
        private static void PoolingDemo()
        {
            var random = new Random(); // Not thread safe!
            var queue = new ConcurrentQueue<int>();

            void Worker()
            {
                var spinWait = new SpinWait();

                while (true)
                {
                    if (queue.TryDequeue(out var number))
                    {
                        Console.WriteLine($"Execution task {number} on thread {Thread.CurrentThread.ManagedThreadId}");
                        spinWait.Reset();
                    }
                    else
                    {
                        spinWait.SpinOnce();
                    }
                }
            }
            
            StartBckThreads(Worker, 3);

            while (true)
            {
                var sleepTime = random.Next(500, 2000);
                Thread.Sleep(sleepTime);

                queue.Enqueue(sleepTime);
            }
        }
        
        private static void LiveDemo()
        {
            var random = new Random(); // Not thread safe!
            var queue = new Queue<int>();

            void Worker()
            {

                while (true)
                {
                    lock (queue)
                    {
                        if (queue.TryDequeue(out var number))
                        {
                            Console.WriteLine($"Execution task {number} on thread {Thread.CurrentThread.ManagedThreadId}");
                        }
                        else
                        {
                            Monitor.Wait(queue);
                        }
                    }
                }
            }
            
            StartBckThreads(Worker, 3);

            while (true)
            {
                var sleepTime = random.Next(500, 2000);
                Thread.Sleep(sleepTime);

                lock (queue)
                {
                    queue.Enqueue(sleepTime);
                    Monitor.Pulse(queue);
                }
            }
        }
        
        private static void PulseWaitDemo()
        {
            var random = new Random(); // Not thread safe!
            var queue = new Queue<int>();
            
            void Worker()
            {
                while (true)
                {
                    var number = 0;
                    lock (queue)
                    {
                        if (!queue.Any())
                        {
                            Monitor.Wait(queue);
                        }

                        number = queue.Dequeue();
                    }
                    
                    Console.WriteLine($"Execution task {number} on thread {Thread.CurrentThread.ManagedThreadId}");
                }
            }

            StartBckThreads(Worker, 3);

            while (true)
            {
                var sleepTime = random.Next(500, 2000);
                Thread.Sleep(sleepTime);

                lock (queue)
                {
                    queue.Enqueue(sleepTime);
                    Monitor.Pulse(queue);
                } // Только после этой строки Worker выйдет из метода Wait
            }
        }

        private static void PulseBeforeWait()
        {
            var lockObject = new object();
            
            var thread = StartBckThread(() =>
            {
                Thread.Sleep(1000);
                Console.WriteLine("Before wait");
                lock (lockObject) Monitor.Wait(lockObject);
                Console.WriteLine("After wait wait");
            });

            Console.WriteLine("Before pulse");
            lock (lockObject) Monitor.Pulse(lockObject);
            Console.WriteLine("After pulse");

            thread.Join();
        }

        private static void ThreadPoolDemo()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object _) { Console.WriteLine(100); });
            Thread.Sleep(1000);
        }
        
        private static Thread[] StartBckThreads(Action action, int count)
        {
            return Enumerable.Range(0, count).Select(_ => StartBckThread(action)).ToArray();
        }
        
        private static Thread StartBckThread(Action action)
        {
            var thread = new Thread(() => action())
            {
                IsBackground = true
            };

            thread.Start();
            
            return thread;
        }
    }
}