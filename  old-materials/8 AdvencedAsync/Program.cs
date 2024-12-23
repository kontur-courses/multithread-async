using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedAsync.Examples
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // CancellationLiveDemo();
            // CancellationDemo();
            
            // AsyncAwaitCancellationDemo();
            // CatchCancellationExceptionDemo();

            // CancellationTimeoutDemo();
            // LinkedTokenDemo();
            // RegisterDemo();
            
            MinMaxThreads();
            
            // LockDemo();
            // await AsyncLockDemo();
            
            // AsyncVoidDemo();
            // await AsyncEnumerableDemo();
        }

        private static void CancellationLiveDemo()
        {
            using var cts = new CancellationTokenSource();
            
            var task = new Task(() =>
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    Thread.Sleep(100);
                }
            }, cts.Token);
            
            task.Start();
            Thread.Sleep(200);
            
            cts.Cancel();
            
            Thread.Sleep(200);
            Console.WriteLine(task.Status);
            task.Wait(cts.Token);
        }

        private static void CancellationDemo()
        {
            using var cts = new CancellationTokenSource();

            var task = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);

                    cts.Token.ThrowIfCancellationRequested();
                }
            }, cts.Token);
            
            task.Start();
            cts.Cancel();
            Thread.Sleep(200);
            Console.WriteLine($"{task.Status}");
            task.Wait();
        }

        private static void AsyncAwaitCancellationDemo()
        {
            using var cts = new CancellationTokenSource();

            var task = Foo(cts.Token);
            
            cts.Cancel();
            Thread.Sleep(200);
            Console.WriteLine($"{task.Status}");
            task.Wait(cts.Token);

            static async Task Foo(CancellationToken token)
            {
                while (true)
                {
                    await Task.Delay(100, token);
                    // var x = await File.ReadAllTextAsync("test", token);

                    token.ThrowIfCancellationRequested();
                }
            }
        }

        private static void CatchCancellationExceptionDemo()
        {
            using var cts = new CancellationTokenSource();

            cts.Cancel();
            
            try
            {
                cts.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Canceled!");
            }
            
            Console.WriteLine("After catch!");
        }

        private static void CancellationTimeoutDemo()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            Handler(cts.Token).Wait(cts.Token);
            
            async Task Handler(CancellationToken token)
            {
                while (true)
                {
                    await Task.Delay(100, token);
                    
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        private static void LinkedTokenDemo()
        {
            using var ctsFirst = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var ctsSecond = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var ctsCombined = CancellationTokenSource.CreateLinkedTokenSource(ctsFirst.Token, ctsSecond.Token);

            var task = Handler(ctsCombined.Token);
            ctsSecond.Cancel();
            task.Wait(ctsCombined.Token);
            
            async Task Handler(CancellationToken token)
            {
                while (true)
                {
                    await Task.Delay(100, token);
                    
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        private static void RegisterDemo()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var _ = cts.Token.Register(() => Console.WriteLine("Canceled!"));
            
            var task = Handler(cts.Token);
            cts.Cancel();
            task.Wait(cts.Token);
            
            async Task Handler(CancellationToken token)
            {
                while (true)
                {
                    await Task.Delay(100, token);
                    
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        private static void MinMaxThreads()
        {
            ThreadPool.GetMinThreads(out var minThreads, out var minCpt);
            Console.WriteLine($"MIN: {minThreads}/{minCpt}");
            
            ThreadPool.GetMaxThreads(out var maxThreads, out var maxCpt);
            Console.WriteLine($"MAX: {maxThreads}/{maxCpt}");

            ThreadPool.SetMinThreads(42, minCpt);
            ThreadPool.SetMaxThreads(64, maxCpt);
            
            ThreadPool.GetMinThreads(out minThreads, out minCpt);
            Console.WriteLine($"MIN: {minThreads}/{minCpt}");
            
            ThreadPool.GetMaxThreads(out maxThreads, out maxCpt);
            Console.WriteLine($"MAX: {maxThreads}/{maxCpt}");
        }

        private static void LockDemo()
        {
            var lockObject = new object();
            lock (lockObject)
            {
                Console.WriteLine("Under lock");
            }
        }

        private static async Task AsyncLockDemo()
        {
            var semaphore = new SemaphoreSlim(1, 1);
            var lockTaken = false;
            
            try
            {
                await semaphore.WaitAsync();
                lockTaken = true;
            
                Console.WriteLine($"Under async lock. Thread #{Thread.CurrentThread.ManagedThreadId}. Semaphore.Count = {semaphore.CurrentCount}");
                await Task.Delay(100);
                Console.WriteLine($"Under async lock. Thread #{Thread.CurrentThread.ManagedThreadId}. Semaphore.Count = {semaphore.CurrentCount}");
            }
            finally
            {
                if (lockTaken)
                    semaphore.Release();
            }
        }

        private static async void AsyncVoidDemo()
        {
            await Task.Delay(100);
        }

        private static async Task AsyncEnumerableDemo()
        {
            await foreach (var i in AsyncEnumerable()) 
                Console.WriteLine(i);

            static async IAsyncEnumerable<int> AsyncEnumerable()
            {
                for (var i = 0; i < 100; i++)
                {
                    await Task.Delay((i % 10) * 100);
                    yield return i;
                }
            }
        }
    }
}