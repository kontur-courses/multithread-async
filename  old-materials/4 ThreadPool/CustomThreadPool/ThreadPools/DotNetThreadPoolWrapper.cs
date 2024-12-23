using System;
using System.Threading;

namespace CustomThreadPool
{
    public class DotNetThreadPoolWrapper : IThreadPool
    {
        private long processedTask = 0L;
        
        public void EnqueueAction(Action action)
        {
            ThreadPool.UnsafeQueueUserWorkItem(delegate
            {
                action.Invoke();
                Interlocked.Increment(ref processedTask);
            }, null);
        }

        public long GetTasksProcessedCount() => processedTask;
    }
}