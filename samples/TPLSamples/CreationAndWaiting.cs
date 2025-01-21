using System;
using System.Threading;
using System.Threading.Tasks;

namespace TPLSamples
{
    public static class CreationAndWaiting
    {
        public static void CreateAndWait()
        {
            var task = new Task(() =>
            {
                Console.WriteLine("Starting...");
                Thread.Sleep(1000);
                Console.WriteLine("... finished!");
            });
            task.Start();
            task.Wait();
        }

        public static void TaskRun()
        {
            Action action = () =>
            {
                Console.WriteLine("Starting...");
                Thread.Sleep(1000);
                Console.WriteLine("... finished!");
            };

            var task = Task.Run(action);
            task.Wait();

            //the code above is equal to

            task = Task.Factory.StartNew(action, TaskCreationOptions.DenyChildAttach);
            task.Wait();
        }

        public static void ParametrizedTask()
        {
            var task = new Task<long>(() => Helper.Fibonacci(42));
            task.Start();
            Console.WriteLine(task.Result);
        }

        public static void TaskFromResult()
        {
            Task<int> task = Task.FromResult(new Random().Next());
            Console.WriteLine(task.Result);
        }

        public static void WaitAllWaitAny()
        {
            var firstTask = Task.Run(() =>
            {
                Console.WriteLine("Task 0 starting...");
                Thread.SpinWait(10000000);
                Console.WriteLine("Task 0 finished...");
            });

            var secondTask = Task.Run(() =>
            {
                Console.WriteLine("Task 1 starting...");
                Thread.SpinWait(100000000);
                Console.WriteLine("Task 1 finished...");
            });

            // Also check Debug -> Windows -> Tasks
            var finishedTaskIndex = Task.WaitAny(firstTask, secondTask);
            Console.WriteLine("Continuation after task {0} finished", finishedTaskIndex);

            Task.WaitAll(firstTask, secondTask);
            Console.WriteLine("All tasks finished");
        }

        public static void WhenAllWhenAny()
        {
            Task.Run(() =>  Thread.SpinWait(1000000000));
            var firstTask = Task.Run(() =>
            {
                Console.WriteLine("Task 0 starting...");
                Thread.SpinWait(10000000);
                Console.WriteLine("Task 0 finishing...");
                return 0;
            });
            var secondTask = Task.Run(() =>
            {
                Console.WriteLine("Task 1 starting...");
                Thread.SpinWait(1000000000);
                Console.WriteLine("Task 1 finishing...");
                return 1;
            });

            Task<Task<int>> whenAnyTask = Task.WhenAny(firstTask, secondTask);
            whenAnyTask.Wait();
            Console.WriteLine("Continuation after task {0} finished", whenAnyTask.Result == firstTask ? "0" : "1");

            Task<int[]> whenAllTask = Task.WhenAll(firstTask, secondTask);
            whenAllTask.Wait();
            Console.WriteLine("All tasks finished: " + string.Join(", ", whenAllTask.Result));
        }

        public static void Statuses()
        {
            var task = new Task(() =>
            {
                Console.WriteLine("Starting...");
                Thread.Sleep(3000);
                Console.WriteLine("... finished!");
            });

            Console.WriteLine("Task1 status: {0}", task.Status);
            task.Start();
            Console.WriteLine("Task1 status: {0}", task.Status);
            Thread.Sleep(1000);
            Console.WriteLine("Task1 status: {0}", task.Status);
            task.Wait();
            Console.WriteLine("Task1 status: {0}", task.Status);
        }
    }
}