using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataParallelism
{
	class Program
	{
		static void Main()
		{
			PlinqDemo();
			//CompareDifferentApproaches();

			//PrimesAndPartitioner();

			//ParallelInvokeDemo();
			//ParallelInvokeOptionsDemo();
			//ParallelInvokeExceptionDemo();

			//ParallelForeachDemo();
			//ParallelForEachWithLocalInitAndLocalFinallyDemo();

			//ParallelForImageDemo();
			//ParallelForStateDemo();

			//PartitionerBitmap.BitmapDemo();
		}

		private static void PlinqDemo()
		{
			var random = new Random();

			var sw = Stopwatch.StartNew();
			var array = Enumerable
				.Range(0, 10_000_000)
				.Select(_ => random.Next(0, 16))
				.AsParallel()
				.Select(Fib)
				.ToArray();

			Console.WriteLine(string.Join(", ", array.Take(20)) + ", ...");
			Console.WriteLine(sw.Elapsed);
		}

		private static void CompareDifferentApproaches()
		{
			var N = 100_000_000;

			var sw = Stopwatch.StartNew();
			for(int i = 0; i < N; i++)
				DoNothing(i);
			sw.Stop();

			Console.WriteLine("                 Classic for: " + sw.Elapsed);

			sw.Restart();
			Parallel.For(0, N, DoNothing);
			sw.Stop();

			Console.WriteLine("                Parallel.For: " + sw.Elapsed);

			sw.Restart();
			Parallel.ForEach(Enumerable.Range(0, N), DoNothing);
			sw.Stop();

			Console.WriteLine("            Parallel.ForEach: " + sw.Elapsed);

			sw.Restart();
			var partitioner = Partitioner.Create(0, N, N / Environment.ProcessorCount);
			Parallel.ForEach(partitioner, partition =>
			{
				for(int i = partition.Item1; i < partition.Item2; i++)
					DoNothing(i);
			});
			sw.Stop();

			Console.WriteLine("Parallel.ForEach partitioned: " + sw.Elapsed);

			sw.Restart();
			Enumerable.Range(0, N).AsParallel().ForAll(DoNothing);
			sw.Stop();

			Console.WriteLine("               PLinq chunked: " + sw.Elapsed);

			sw.Restart();
			ParallelEnumerable.Range(0, N).ForAll(DoNothing);
			sw.Stop();

			Console.WriteLine("                PLinq ranged: " + sw.Elapsed);
		}

		public static void PrimesAndPartitioner()
		{
			int border = 300000;
			var sw = Stopwatch.StartNew();

			var primes =
				// =======================================
				Enumerable.Range(3, border - 3).AsParallel()
				// =======================================
				//ParallelEnumerable.Range(3, border - 3)
				// =======================================
				.WithDegreeOfParallelism(8)
				.Where(n => Enumerable.Range(2, /*(int)Math.Sqrt(n)*/ n / 2).All(i => n % i > 0))
				.ToList();

			sw.Stop();

			Console.WriteLine("Primes less than " + border);
			Console.WriteLine("Primes count: " + primes.Count);
			Console.WriteLine(string.Join(", ", primes.Take(5)) + ", ...");
			Console.WriteLine(sw.Elapsed);
		}

		#region Parallel.Invoke

		private static void ParallelInvokeDemo()
		{
			var actions = Enumerable.Range(0, 1_000_000).Select(n => (Action)(() => DoWork(n))).ToArray();

			Parallel.Invoke(actions);
		}

		private static void ParallelInvokeOptionsDemo()
		{
			var actions = Enumerable.Range(0, 1_000_000).Select(n => (Action)(() => DoWork(n))).ToArray();

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
			int count = 0;

			var actions = Enumerable.Range(0, 1000).Select(i => (Action)(() =>
			{
				Console.Write('.');
				Interlocked.Increment(ref count);
			})).ToArray();
			actions[10] = () =>
			{
				Console.Write('.');
				Interlocked.Increment(ref count);
				throw new Exception("!!!");
			};

			var options = new ParallelOptions
			{
				MaxDegreeOfParallelism = 4,
				CancellationToken = CancellationToken.None,
				TaskScheduler = TaskScheduler.Default
			};

			try
			{
				Parallel.Invoke(options, actions);
			}
			catch(Exception e) { Console.WriteLine(e.Message); }
			finally { Console.WriteLine("COUNT: " + count); }
		}

		#endregion

		#region Parallel.ForEach

		private static void ParallelForeachDemo()
		{
			var random = new Random();
			var numbers = Enumerable.Range(0, 100).Select(_ => random.Next(0, 16)).ToArray();

			Parallel.ForEach(numbers, n => Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]\t{Fib(n)}"));
		}

		private static void ParallelForEachWithLocalInitAndLocalFinallyDemo()
		{
			Parallel.ForEach(new[]
				{
					"https://google.com/robots.txt",
					"https://yandex.com/robots.txt",
					"https://yahoo.com/robots.txt",
					"https://bing.com/robots.txt",
					"https://duckduckgo.com/robots.txt"
				},
				() => new WebClient(),
				(url, _, index, client) =>
				{
					client.DownloadFile(url, "robotstxt-" + new Uri(url).DnsSafeHost + ".txt");
					Console.WriteLine("{0}\t[{1}]\t{2}", index, Thread.CurrentThread.ManagedThreadId, url);
					return client;
				}, client => client.Dispose());
		}

		#endregion

		#region Parallel.For

		public static void ParallelForImageDemo()
		{
			using var image = (Bitmap)Image.FromFile("large.png");
			using var bmp = new DirectBitmap(image);

			var sw = Stopwatch.StartNew();

			//for(int y = 0; y < bmp.Height; y++)
			Parallel.For(0, bmp.Height, y =>
			{
				for(int x = 0; x < bmp.Width; x++)
					bmp.FastSetPixel(x, y, bmp.FastGetPixel(x, y).GrayScale());
			});

			sw.Stop();

			Console.WriteLine(sw.Elapsed);
			image.Save("large-gray.png", ImageFormat.Png);
		}

		private static void ParallelForStateDemo()
		{
			var count = 100_000;

			var random = new Random();
			var input = Enumerable.Range(0, count).Select(_ => random.Next(0, 16)).ToArray();
			var output = Enumerable.Range(0, count).ToArray();

			var result = Parallel.For(0, input.Length, (i, state) =>
			{
				if((output[i] = Fib(input[i])) > 100)
					state.Break();
			});

			Console.WriteLine("Input: " + string.Join(", ", input.Take(100)));
			Console.WriteLine($"Completed '{result.IsCompleted}', lowest break iteration '{result.LowestBreakIteration?.ToString() ?? "null"}', input '{(result.LowestBreakIteration == null ? "n/a" : input[result.LowestBreakIteration ?? 0].ToString())}', output '{(result.LowestBreakIteration == null ? "n/a" : output[result.LowestBreakIteration ?? 0].ToString())}'");
		}

		#endregion

		[ThreadStatic] private static long dummy;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DoNothing(int i) { dummy += i; }

		private static int Fib(int n)
		{
			return n switch
			{
				0 => 0,
				1 => 1,
				_ => Fib(n - 1) + Fib(n - 2)
			};
		}

		private static void DoWork(int num)
		{
			Thread.SpinWait(num % 100);
			if(num % 100_000 == 0)
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]\t{num}");
		}
	}
}
