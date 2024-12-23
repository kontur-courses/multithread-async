using System;

namespace CustomThreadPool
{
    public interface IThreadPool
    {
        void EnqueueAction(Action action);
        long GetTasksProcessedCount();
    }
}