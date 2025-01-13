using System;

namespace ReaderWriterLock
{
	public class LockWrapper : IRwLock
	{
		public void ReadLocked(Action action)
		{
			lock(lockObject)
				action();
		}

		public void WriteLocked(Action action)
		{
			lock(lockObject)
				action();
		}

		private readonly object lockObject = new object();
	}
}