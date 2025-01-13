using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSamples
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // var evnt = WriteFile_APM();
                //
                // Console.WriteLine($"Main thread still working [thread {Thread.CurrentThread.ManagedThreadId}]");
                // evnt.WaitOne();
                // Console.WriteLine($"Main thread finishing");


                Task task;
                // task = FileContinueWith();
                // task = WriteFile_Async();
                // task = FileCompareAsync();
                // task = LambdaAsync();
                task = DownloadWebPageAsync();

                Console.WriteLine($"Main thread still working [thread {Thread.CurrentThread.ManagedThreadId}]");
                task.Wait();
                Console.WriteLine("Main thread finishing");
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private const int WRITE_DATA_SIZE = 128 * 1024 * 1024;

        public static WaitHandle WriteFile_APM()
        {
            var data = new byte[WRITE_DATA_SIZE];
            new Random().NextBytes(data);
            Console.WriteLine("Generated random data");
            var fStream = new FileStream("file1.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            var sw = Stopwatch.StartNew();
            var asyncResult = fStream.BeginWrite(data, 0, data.Length,
                asyncResult =>
                {
                    var fs = (FileStream)asyncResult.AsyncState;
                    fs.EndWrite(asyncResult);
                    fs.Dispose();
                    Console.WriteLine($"Finished disk I/O! in {sw.ElapsedMilliseconds} ms [thread {Thread.CurrentThread.ManagedThreadId}]");
                }, fStream);
            Console.WriteLine($"Started disk I/O! in {sw.ElapsedMilliseconds} ms total [thread {Thread.CurrentThread.ManagedThreadId}]");

            return asyncResult.AsyncWaitHandle;
        }


        public static Task WriteFile_ContinueWith()
        {
            var buf = new byte[WRITE_DATA_SIZE];
            new Random().NextBytes(buf);
            Console.WriteLine("Generated random data");
            var fStream = new FileStream("file2.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            var sw = Stopwatch.StartNew();
            var copyToAsyncTask = fStream.WriteAsync(buf, 0, buf.Length);
            Console.WriteLine($"Started disk I/O! in {sw.ElapsedMilliseconds} ms [thread {Thread.CurrentThread.ManagedThreadId}]");

            var resultTask = copyToAsyncTask
                .ContinueWith(writeTask =>
                {
                    fStream.Dispose();
                    if(writeTask.IsFaulted)
                        throw writeTask.Exception;
                    Console.WriteLine($"Finished disk I/O! in {sw.ElapsedMilliseconds} ms total [thread {Thread.CurrentThread.ManagedThreadId}]");
                });

            return resultTask;
        }


        public static async Task WriteFile_Async()
        {
            var buf = new byte[WRITE_DATA_SIZE];
            new Random().NextBytes(buf);
            Console.WriteLine("Generated random data");
            var fStream = new FileStream("file3.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            var sw = Stopwatch.StartNew();
            var writeAsyncTask = fStream.WriteAsync(buf, 0, buf.Length);
            Console.WriteLine($"Started disk I/O! in {sw.ElapsedMilliseconds} ms [thread {Thread.CurrentThread.ManagedThreadId}]");

            await writeAsyncTask;
            await fStream.DisposeAsync();

            Console.WriteLine($"Finished disk I/O! in {sw.ElapsedMilliseconds} ms total [thread {Thread.CurrentThread.ManagedThreadId}]");
        }


        public static async Task FileCompareAsync()
        {
            var sw = Stopwatch.StartNew();

            var taskZeros1 = HashFileAsync("file1.txt");
            Console.WriteLine($"Started async read of first file [thread {Thread.CurrentThread.ManagedThreadId}]");

            var taskZeros2 = HashFileAsync("file2.txt");
            Console.WriteLine(format: "Started async read of second file [thread {0}]", Thread.CurrentThread.ManagedThreadId);

            Console.WriteLine($" Started async reads in {sw.ElapsedMilliseconds} ms [thread {Thread.CurrentThread.ManagedThreadId}]");

//			var hash1 = await taskZeros1;
//			Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
//			var hash2 = await taskZeros2;
//			Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            //или так
            var hashes = await Task.WhenAll(taskZeros1, taskZeros2);
            var hash1 = hashes[0];
            var hash2 = hashes[1];

            Console.WriteLine($"Hash1: {hash1} [thread {Thread.CurrentThread.ManagedThreadId}]");
            Console.WriteLine($"Hash2: {hash2} [thread {Thread.CurrentThread.ManagedThreadId}]");
        }
        private static async Task<string> HashFileAsync(string filename)
        {
            using(var ms = new MemoryStream())
            {
                var sw = Stopwatch.StartNew();
                using(var ifs = new FileStream(filename, FileMode.Open))
                {
                    Console.WriteLine($"Starting async read of file {filename} [thread {Thread.CurrentThread.ManagedThreadId}]");
                    await ifs.CopyToAsync(ms);
                }
                Console.WriteLine($"Read file {filename} in {sw.ElapsedMilliseconds} ms  [thread {Thread.CurrentThread.ManagedThreadId}]");

                sw.Restart();
                using var md5 = MD5.Create();
                var hash = await md5.ComputeHashAsync(ms);
                Console.WriteLine($"Computed hash of file {filename} in {sw.ElapsedMilliseconds} ms [thread {Thread.CurrentThread.ManagedThreadId}]");

                return Convert.ToBase64String(hash);
            }
        }


        public static async Task LambdaAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            double result = 0;

            result = await CosCalcAsync();
            Console.WriteLine($"Finished with {result} in {sw.Elapsed.TotalMilliseconds}ms");

            sw.Restart();
            await Task.WhenAll(Enumerable.Range(1, 100).Select(_ => CosCalcAsync()));
            Console.WriteLine($"Finished all in {sw.Elapsed.TotalMilliseconds}ms");
        }
        private static Task<double> CosCalcAsync()
        {
            return Task.Run(async () =>
            {
                double cur = 1;
                for(int i = 0; i < 1 * 1000 * 1000; i++)
                {
                    cur = Math.Cos(cur);
                }

                await Task.Delay(1000);
                return cur;
            });
        }


        public static async Task DownloadWebPageAsync()
        {
            var sw = Stopwatch.StartNew();
            var httpClient = new HttpClient();
            
            using var content = await httpClient.GetStreamAsync("http://e1.ru");
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            Console.WriteLine("Got {0} bytes in {1} ms", ms.Position, sw.ElapsedMilliseconds);
            Console.WriteLine(Encoding.GetEncoding(1251).GetString(ms.ToArray()));
        }
    }
}