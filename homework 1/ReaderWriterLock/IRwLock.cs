using System;

namespace ReaderWriterLock
{
	public interface IRwLock
	{
		void ReadLocked(Action action);
		void WriteLocked(Action action);
	}
}