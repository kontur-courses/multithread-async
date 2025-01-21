using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TPLSamples
{
    public static class Continuation
    {
        public static void Parent()
        {
            var parent = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Outer task executing...");
                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine("Nested task executing...");
                    Thread.Sleep(1000);
                    Console.WriteLine("Nested task completing...");
                }, TaskCreationOptions.AttachedToParent);
            }/*, TaskCreationOptions.DenyChildAttach*/);

            Thread.Sleep(100);
            Console.WriteLine("Outer task status {0}", parent.Status);

            parent.Wait();
            Console.WriteLine("Outer has completed");
        }

        public static void ContinueWith()
        {
            var tasksChain = Task.Run(() => Console.WriteLine("Starting in thread #{0}", Thread.CurrentThread.ManagedThreadId))
                .ContinueWith(previousTask =>
                {
                    Console.WriteLine("Sleeping in thread #{0}", Thread.CurrentThread.ManagedThreadId);
                    Thread.Sleep(1000);
                    // Task.Run(() => { Thread.Sleep(1000); });
                })
                .ContinueWith(previousTask =>
                {
                    Console.WriteLine("After sleeping in thread #{0}", Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine("SleepingTask status is {0}", previousTask.Status);
                }/*, TaskContinuationOptions.ExecuteSynchronously*/);

            tasksChain.Wait();
        }

        public static void MultipleContinuations()
        {
            var sw = Stopwatch.StartNew();
            var task1 = Task.Run(() => Console.WriteLine("Starting in thread #{0}", Thread.CurrentThread.ManagedThreadId));
            var task2 = task1
                .ContinueWith(previousTask =>
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("Sleeping 1 in thread #{0}", Thread.CurrentThread.ManagedThreadId);
                    Thread.Sleep(1000);
                }/*, TaskContinuationOptions.ExecuteSynchronously*/);
            var task3 = task1
                .ContinueWith(previousTask =>
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Sleeping 2 in thread #{0}", Thread.CurrentThread.ManagedThreadId);
                    Thread.Sleep(1000);
                }/*, TaskContinuationOptions.ExecuteSynchronously*/);
            var task4 = task1
                .ContinueWith(previousTask =>
                {
                    Console.WriteLine("Sleeping 3 in thread #{0}", Thread.CurrentThread.ManagedThreadId);
                    Thread.Sleep(1000);
                }/*, TaskContinuationOptions.ExecuteSynchronously*/);

            Task.WaitAll(task1, task2, task3, task4);
            Console.WriteLine("Elapsed: {0}", sw.Elapsed);
        }

        public static void TaskStatusWhenContinueWith()
        {
            var task = Task.Run(() =>
            {
                Console.WriteLine("Sleping in thread #{0}", Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1000);
            });

            var continuationTask = task.ContinueWith(_ =>
                Console.WriteLine("Finished sleeping in thread #{0}", Thread.CurrentThread.ManagedThreadId));

            Thread.Sleep(500);

            Console.WriteLine("ContinuationTask status is {0}", continuationTask.Status);
            continuationTask.Wait();
        }

        public static void ContinueWhenAllWhenAny()
        {
            var task1 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                Console.WriteLine("Task 1 done");
                return 1;
            });

            var task2 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                Console.WriteLine("Task 2 done");
                return 2;
            });

            var whenAnyTaks = Task.Factory.ContinueWhenAny(new[] {task1, task2}, task =>
                Console.WriteLine("After task {0} done", task.Result));
            whenAnyTaks.Wait();
            Console.WriteLine("At least one task is done");

            var resultTask = Task.Factory.ContinueWhenAll(new[] {task1, task2}, tasks =>
                tasks.Sum(t => t.Result));
            Console.WriteLine("Result: {0}", resultTask.Result);
        }
    }
}