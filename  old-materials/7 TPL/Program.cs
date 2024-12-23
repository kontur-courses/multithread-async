using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwait.Examples
{
    public static class Program
    {
        private const int WriteBlockSize = 16 * 1024;
        private const int WriteDataSize = 128 * 1024 * 1024;

        private static string RandomFileName => Path.GetTempFileName();

        public static void Main(string[] args)
        {
            var data = new byte[WriteDataSize];
            new Random().NextBytes(data);

            SyncWriteDemo(data);
            // ApmDemo(data);
            // TplDemo(data);
            
            // TplChainDemo(data);
            // AsyncAwaitLiveDemo(data);
            // AsyncAwaitDemo(data);
            
            // AwaitException();
            // LambdaDemo();
            
            // AggregateExceptionDemo();
            // MultipleExceptionsDemo();
            // OriginalExceptionDemo();
        }

        private static void SyncWriteDemo(byte[] data)
        {
            var sw = Stopwatch.StartNew();
            Console.WriteLine($"[SYNC] Started disk I/O! (thread {Thread.CurrentThread.ManagedThreadId})");
            File.WriteAllBytes(RandomFileName, data);
            Console.WriteLine($"[SYNC] Finished disk I/O! in {sw.ElapsedMilliseconds} ms (thread {Thread.CurrentThread.ManagedThreadId})");
        }

        private static void ApmDemo(byte[] data)
        {
            var @event = Write(data);
            Console.WriteLine("[APM] Start waiting");
            @event.WaitOne();
            Console.WriteLine("[APM] Operation completed");

            static AutoResetEvent Write(byte[] data)
            {
                var @event = new AutoResetEvent(false);

                var stream = new FileStream(RandomFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, WriteBlockSize, FileOptions.WriteThrough);
                var sw = Stopwatch.StartNew();

                stream.BeginWrite(data, 0, data.Length, asyncResult =>
                {
                    ((FileStream) asyncResult.AsyncState)!.EndWrite(asyncResult);
                    @event.Set();
                    stream.Dispose();
                    Console.WriteLine($"[APM] Finished disk I/O! in {sw.ElapsedMilliseconds} ms (thread {Thread.CurrentThread.ManagedThreadId})");
                }, stream);

                Console.WriteLine($"[APM] Started disk I/O! (thread {Thread.CurrentThread.ManagedThreadId})");

                return @event;
            }
        }

        private static void TplLiveDemo(byte[] data)
        {
            var task = File.WriteAllBytesAsync(RandomFileName, data)
                .ContinueWith(t => Console.WriteLine($"Done {t.Exception?.InnerException?.GetType().Name}"));

            task.Wait();
        }

        private static void TplDemo(byte[] data)
        {
            var sw = Stopwatch.StartNew();
            
            var task = File.WriteAllBytesAsync(RandomFileName, data)
                .ContinueWith(_ => Console.WriteLine($"[TPL] Finished disk I/O! in {sw.ElapsedMilliseconds} ms (thread {Thread.CurrentThread.ManagedThreadId})"));
                
            Console.WriteLine($"[TPL] Started disk I/O! (thread {Thread.CurrentThread.ManagedThreadId})");
            
            Console.WriteLine("[TPL] Start waiting");
            task.Wait();
            Console.WriteLine("[TPL] Operation completed");
        }

        private static void TplChainDemo(byte[] data)
        {
            WriteReadWriteAsync(RandomFileName, data).Wait();

            static Task WriteReadWriteAsync(string file, byte[] data)
            {
                Console.WriteLine("Before");

                return File.WriteAllBytesAsync(file, data)
                    .ContinueWith(_ =>
                    {
                        File.ReadAllBytesAsync(file)
                            .ContinueWith(readTask =>
                            {
                                var content = readTask.Result;
                                for (var i = 0; i < content.Length; i++) 
                                    content[i] ^= 42;
                                File.WriteAllBytesAsync(file, data)
                                    .ContinueWith(_ => Console.WriteLine("After"), TaskContinuationOptions.AttachedToParent);
                            }, TaskContinuationOptions.AttachedToParent);
                    });
            }
        }

        private static void AsyncAwaitLiveDemo(byte[] data)
        {
            WriteReadWriteAsync(RandomFileName, data).Wait();

            static async Task WriteReadWriteAsync(string file, byte[] data)
            {
                Console.WriteLine("Before");

                await File.WriteAllBytesAsync(file, data);
                
                var content = await File.ReadAllBytesAsync(file);
                
                for (var i = 0; i < content.Length; i++) 
                    content[i] ^= 42;

                await File.WriteAllBytesAsync(file, content);
                
                Console.WriteLine("After");
            }
        }

        private static void AsyncAwaitDemo(byte[] data)
        {
            WriteReadWriteAsync(RandomFileName, data).Wait();
        
            static async Task WriteReadWriteAsync(string file, byte[] data)
            {
                Console.WriteLine("(1)");
                
                await File.WriteAllBytesAsync(file, data);

                Console.WriteLine("(2)");
                
                var content = await File.ReadAllBytesAsync(file);
                
                for (var i = 0; i < content.Length; i++) 
                    content[i] ^= 42;

                Console.WriteLine("(3)");
                
                await File.WriteAllBytesAsync(file, data);
                
                Console.WriteLine("(4)");
            }
        }

        private static void AwaitException()
        {
            Foo().Wait();
            
            static async Task Foo()
            {
                try
                {
                    int num = await GetIntAsync();
                    
                    await ThrowException();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"[{e.GetType().Name}]\n{e}");
                }

                lock (RandomFileName)
                {
                    // await ThrowException();
                }
            }

            static async Task ThrowException() => throw new InvalidOperationException("Bla bla bla");

            static async Task<int> GetIntAsync() => 42;
        }

        interface IFoo
        {
            Task GooAsync();
            Task<int> BooAsync();
        }

        class Foo : IFoo
        {
            public async Task GooAsync()
            {
                await File.WriteAllTextAsync("Test", "42");
            }

            public async Task<int> BooAsync()
            {
                return int.Parse(await File.ReadAllTextAsync("Test"));
            }
        }

        private static void LambdaDemo()
        {
            LambdaDemoAsync().Wait();
            
            static async Task LambdaDemoAsync()
            {
                var writeTasks = Enumerable.Range(0, 10)
                    .Select(async i =>
                    {
                        await File.WriteAllTextAsync(i.ToString(), i.ToString());
                    });

                Task task = Task.WhenAll(writeTasks);
                await task;

                var readTasks = Enumerable.Range(0, 10)
                    .Select(async i => int.Parse(await File.ReadAllTextAsync(i.ToString())));

                var results = await Task.WhenAll(readTasks);
                
                Console.WriteLine(results.Sum());
            }
        }
        
        private static void AggregateExceptionDemo()
        {
            var task1 = Task.Run(() =>
            {
                throw new InvalidOperationException("Oops 1");
                return 42;
            });

            try
            {
                task1.Wait();
                var x = task1.Result;
            }
            catch (AggregateException e)
            {
                Console.WriteLine(e.InnerExceptions.Count);
                Console.WriteLine(e.InnerException);
            }
        }

        private static void MultipleExceptionsDemo()
        {
            var task1 = Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() => throw new InvalidOperationException("1"), TaskCreationOptions.AttachedToParent);
                Task.Factory.StartNew(() => throw new FileNotFoundException("2"), TaskCreationOptions.AttachedToParent);
            });

            try
            {
                task1.Wait();
                // var x = task1.Result;
            }
            catch (AggregateException e)
            {
                Console.WriteLine(e.InnerExceptions.Count);
                
                e/*.Flatten()*/.Handle(e =>
                {
                    Console.WriteLine(e.GetType().Name);
                    return e.GetType() == typeof(FileNotFoundException);
                });
            }
        }

        private static void OriginalExceptionDemo()
        {
            var task1 = Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() => throw new InvalidOperationException("1"), TaskCreationOptions.AttachedToParent);
                // throw new InvalidOperationException("1");
            });

            Foo().Wait();
            
            async Task Foo()
            {
                try
                {
                    await task1;
                    task1.GetAwaiter().GetResult();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Invalid operation!");
                }
                catch (AggregateException e)
                {
                    Console.WriteLine("Aggregate exception!");
                }
            }
        }
    }
}