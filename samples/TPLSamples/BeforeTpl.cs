using System;
using System.Threading;

namespace TPLSamples
{
	static class BeforeTpl
	{
        public static void QueueUserWorkItem()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Console.WriteLine("Starting...");
                Thread.Sleep(1000);
                Console.WriteLine("... finished!");
            });
        }

        public static void QueueUserWorkItemWaitingToFinish()
        {
            var methodFinishedEvent = new ManualResetEventSlim(false);
            ThreadPool.QueueUserWorkItem(state =>
            {
                Console.WriteLine("Starting...");
                Thread.Sleep(1000);
                Console.WriteLine("... finished!");
                methodFinishedEvent.Set();
            });
            methodFinishedEvent.Wait();
            Console.WriteLine("Continuation after work done");
        }
    }
}