namespace TPLSamples
{
	static class Helper
	{
		public static long Fibonacci(long n)
		{
			switch(n)
			{
				case 0: return 0;
				case 1: return 1;
				default: return Fibonacci(n - 1) + Fibonacci(n - 2);
			}
		}
	}
}