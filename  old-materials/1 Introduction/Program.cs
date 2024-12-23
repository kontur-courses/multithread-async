using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //ProcessDemo();
            // ProcessAffinity();
            // PriorityClass();
            // ThreadDemo();
            // ThreadExceptionDemo(false);
            // ThreadExceptionDemo(true);
            GlobalExceptionHandling();
        }

        private static void ProcessDemo()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\tree.com",
                    // Arguments = "1 2 3",
                    // WorkingDirectory = @"C:\AnotherDirectory",
                    RedirectStandardOutput = true,
                    // RedirectStandardInput = true,
                    // RedirectStandardError = true,
                    // StandardOutputEncoding = Encoding.UTF8,
                    // StandardInputEncoding = Encoding.UTF8,
                    // StandardErrorEncoding = Encoding.UTF8,
                }
            };
            
            process.Start();
            process.WaitForExit();
            Console.WriteLine("Output from another process:");
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }

        private static void ProcessAffinity()
        {
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr) (1 << 0 | 1 << 1);
        }

        private static void PriorityClass()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }

        private static void ThreadDemo()
        {
            var thread = new Thread(() => Console.WriteLine($"Hello from another thread: {Thread.CurrentThread.ManagedThreadId}"))
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            thread.Start();
            thread.Join();
        }

        private static void ThreadExceptionDemo(bool background)
        {
            var thread = new Thread(() =>
            {
                Thread.Sleep(1000);
                throw new Exception($"Exception from {((background ? "background" : "foreground"))} thread: {Thread.CurrentThread.ManagedThreadId}");
            }) {IsBackground = background};
            thread.Start();

            while (true)
            {
                Console.WriteLine($"Hello from main thread: {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(100);
            }
        }

        private static void GlobalExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                while (true)
                {
                    Console.WriteLine($"Exception! {args.ExceptionObject.GetType().Name}: {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(1000);
                }
            };
            ThreadExceptionDemo(false);
        }
    }
}