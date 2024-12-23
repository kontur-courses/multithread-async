using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataParallelism.Examples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // ParallelInvokeDemo();
            // ParallelInvokeOptionsDemo();
            // ParallelInvokeExceptionDemo();

            // ParallelForDemo();
            // ParallelForStateDemo();
            // ParallelForExceptionDemo();
            
            // ParallelForeachDemo();

            // LinqDemo();
            // PlinqDemo();
            PlinqOrderDemo();
            // PlinqMethodsDemo();

            // PartitionerDemo();
        }

        private static void ParallelInvokeDemo()
        {
            var actions = Enumerable.Range(0, 1_000_000).Select(n => (Action) (() => Foo(n))).ToArray();
            
            Parallel.Invoke(actions);
        }
        
        private static void ParallelInvokeOptionsDemo()
        {
            var actions = Enumerable.Range(0, 1_000_000).Select(n => (Action) (() => Foo(n))).ToArray();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 1, 
                CancellationToken = CancellationToken.None,
                TaskScheduler = TaskScheduler.Default
            };
            
            Parallel.Invoke(options, actions);
        }
        
        private static void ParallelInvokeExceptionDemo()
        {
            var actions = new Action[]
            {
                () => Console.WriteLine("1"),
                () =>
                {
                    Console.WriteLine("2");
                    throw new InvalidOperationException();
                },
                () => Console.WriteLine("3"),
            };

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 1, 
                CancellationToken = CancellationToken.None,
                TaskScheduler = TaskScheduler.Default
            };
            
            Parallel.Invoke(options, actions);
        }
        
        private static void ParallelForDemo()
        {
            var random = new Random();
            var array = Enumerable.Range(0, 10_000).Select(_ => random.Next(0, 16)).ToArray();

            //=============================================================
            for (var i = 0; i < array.Length; i++)
                array[i] = Fib(array[i]);
            //=============================================================
            // Parallel.For(0, array.Length, i => array[i] = Fib(array[i]));
            //=============================================================

            Console.WriteLine(string.Join("\n", array.Take(1000)));
        }

        private static void ParallelForStateDemo()
        {
            var random = new Random();
            var array = Enumerable.Range(0, 10_000).Select(_ => random.Next(0, 16)).ToArray();

            var result = Parallel.For(0, array.Length, (i, state) =>
            {
                if ((array[i] = Fib(array[i])) > 10) 
                    state.Break();
            });

            Console.WriteLine(string.Join("\n", array.Take(1000)));
            
            Console.WriteLine($"{result.IsCompleted} {result.LowestBreakIteration}");
        }

        private static void ParallelForExceptionDemo()
        {
            Parallel.For(0, 100, new ParallelOptions{MaxDegreeOfParallelism = 2}, (n, state) =>
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}\t{n}");
                
                if (n == 10)
                    throw new Exception("Oops");
            });
        }

        private static void ParallelForeachDemo()
        {
            var random = new Random();
            var numbers = Enumerable.Range(0, 100).Select(_ => random.Next(0, 16)).ToArray();

            Parallel.ForEach(numbers, n => Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}\t{Fib(n)}"));
        }

        private static void LinqDemo()
        {
            var random = new Random();
            
            var array = Enumerable
                .Range(0, 10_000)
                .Select(_ => random.Next(0, 16))
                .Select(Fib)
                .ToArray();
        }
        

        private static void PlinqDemo()
        {
            var random = new Random();
            
            var array = Enumerable
                .Range(0, 10_000)
                .Select(_ => random.Next(0, 16))
                .AsParallel()
                .Select(Fib)
                .ToArray();
        }

        private static void PlinqOrderDemo()
        {
            var result = Enumerable
                .Range(0, 100)
                .AsParallel()
                
                // .AsUnordered() // defult
                // .AsOrdered() // Гарантирует порядок результата, но не порядок выполнения
                
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                
                .Select(i => i + 1)
                .ToArray();
            
            Console.WriteLine(string.Join(", ", result));
        }
        
        private static void PlinqMethodsDemo()
        {
            Enumerable
                .Range(0, 100)
                .AsParallel()

                .WithCancellation(CancellationToken.None)
                .WithDegreeOfParallelism(10)

                // .AsSequential() // Последующий запрос должен выполняться последовательно

                .ForAll(Console.WriteLine);
        }

        #region Advanced
        private static void PartitionerDemo()
        {
            var numbers = Enumerable.Range(0, 100).ToArray();

            Parallel.ForEach(
                Partitioner.Create(numbers.Select(n => n), EnumerablePartitionerOptions.NoBuffering),
                new ParallelOptions {MaxDegreeOfParallelism = 2},
                i => numbers[i] = Thread.CurrentThread.ManagedThreadId);

            Partitioner
                .Create(numbers)
                .AsParallel()
                .WithDegreeOfParallelism(100)
                .ForAll(Console.WriteLine);
            
            Console.WriteLine(string.Join(", ", numbers));
        }

        private class MyPartitioner<T> : Partitioner<T>
        {
            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
            {
                throw new NotImplementedException();
            }
        }
        #endregion
        
        private static int Fib(int n)
        {
            return n switch
            {
                0 => 0,
                1 => 1,
                _ => Fib(n - 1) + Fib(n - 2)
            };
        }
        
        private static void Foo(int num)
        {
            Thread.SpinWait(num % 100);
            if (num % 100_000 == 42)
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}\t{num}");
        }
    }
}