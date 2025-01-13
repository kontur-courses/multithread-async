using System;
using System.Threading;

namespace ReaderWriterLock
{
	public class ReaderWriterLockWrapper : IRwLock
	{
		public void ReadLocked(Action action)
		{
			rwLock.EnterReadLock();
			try { action(); } finally { rwLock.ExitReadLock(); }
		}

		public void WriteLocked(Action action)
		{
			rwLock.EnterWriteLock();
			try { action(); } finally { rwLock.ExitWriteLock(); }
		}

		private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
	}
}