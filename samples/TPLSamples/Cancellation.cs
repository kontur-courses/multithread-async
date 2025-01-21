using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TPLSamples
{
	static class Cancellation
	{
		public static void Cancel()
		{
			var cancelSource = new CancellationTokenSource();
			var task = Task.Factory.StartNew(() =>
			{
				Console.WriteLine("Staring...");
				Thread.Sleep(1000);
				Console.WriteLine("Before check cancellation");
				cancelSource.Token.ThrowIfCancellationRequested();
				/*Enumerable.Range(34, 10)
					.AsParallel()
					.AsOrdered()
					.WithMergeOptions(ParallelMergeOptions.NotBuffered)
					.WithCancellation(cancelSource.Token)
					.Select(i => (i, Helper.Fibonacci(i)))
					.ForAll(tuple => Console.WriteLine(tuple.i));*/
				Console.WriteLine("After check cancellation");
				Thread.Sleep(1000);
				Console.WriteLine("...finished!");
			}, cancelSource.Token);

			Thread.Sleep(500);

			cancelSource.Cancel();
			task.ContinueWith(_ => Console.WriteLine("Task status: {0}", task.Status)).Wait();
		}
	}
}