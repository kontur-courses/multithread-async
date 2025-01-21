using System;
using System.Threading;
using System.Threading.Tasks;

namespace TPLSamples
{
    public static class Exceptions
    {
        public static void WaitAndStatus()
        {
            var crashingTask = Task.Run(() => throw new Exception("haha!"));

            try
            {
                crashingTask.Wait();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("CrashingTask status is {0}", crashingTask.Status);
        }

        public static void ContinueWith()
        {
            var crashingTask = Task.Run(() => throw new Exception("haha!"));

            var continuationTask = crashingTask
                .ContinueWith(t => Console.WriteLine("CrashingTask status is {0}", t.Status)
                /*, TaskContinuationOptions.OnlyOnRanToCompletion*/);

            continuationTask.Wait();

            Console.WriteLine("ContinuationTask status is {0}", continuationTask.Status);
        }

        public static void HandleAndFlatten()
        {
            var firstCrashingTask = Task.Run(() =>
            {
                Thread.Sleep(2000);
                throw new Exception("First");
            });
            var secondCrashingTask = Task.Run(() => throw new Exception("Second"));
            var thirdCrashingTask = Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() => throw new Exception("Third child"),
                    TaskCreationOptions.AttachedToParent);

                throw new Exception("Third");
            });

            var whenAllTask = Task.WhenAll(firstCrashingTask, secondCrashingTask, thirdCrashingTask);

            whenAllTask.ContinueWith(_ => { }).Wait();

            whenAllTask.Exception?/*.Flatten()*/.Handle(e =>
            {
                Console.WriteLine("{0}: {1}", e.GetType(), e.Message);
                return true;
            });
        }
    }
}