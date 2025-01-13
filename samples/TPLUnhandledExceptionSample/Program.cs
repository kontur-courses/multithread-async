using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TPLUnhandledExceptionSample
{
	static class Program
	{
		static void Main()
		{
			/*TaskScheduler.UnobservedTaskException += (sender, e) =>
			{
				Console.WriteLine(e.Exception.InnerExceptions.First().Message);
				e.SetObserved();
			};*/

			UnhandledException();

			Thread.Sleep(2000);

			/*GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();*/

			Console.WriteLine("==== End of program ====");
		}

		private static void UnhandledException()
		{
			var task = Task.Factory.StartNew(() =>
			{
				Console.WriteLine("Starting...");
				Thread.Sleep(1000);
				throw new Exception("OOPS!");
			});
			task.ContinueWith(t =>
			{
				Console.WriteLine("Continuation...");
				// Console.WriteLine(t.Exception?.Flatten().InnerExceptions.FirstOrDefault()?.Message);
			});
		}
	}
}