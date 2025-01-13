using System;

namespace ClusterTests
{
	public static class DoIt
	{
		public static bool Try(this Action action, out Exception ex)
		{
			try
			{
				action();
				ex = null;
				return true;
			}
			catch (Exception e)
			{
				ex = e;
				return false;
			}
		}
	}
}