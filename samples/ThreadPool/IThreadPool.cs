using System;

namespace ThreadPool
{
	public interface IThreadPool : IDisposable
	{
		void EnqueueAction(Action action);
	}
}