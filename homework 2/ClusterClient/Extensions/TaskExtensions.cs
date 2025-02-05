using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClusterClient.Extensions;

public static class TaskExtensions
{
    public static async Task<Task<T>> WhenAnyCompleteSuccessfully<T>(this IEnumerable<Task<T>> tasks, TimeSpan timeout)
    {
        var timeoutTask = CreateTaskDelayWithResult<T>(timeout);
        var hashedTasks = tasks
            .Append(timeoutTask)
            .ToHashSet();

        while (hashedTasks.Count > 1)
        {
            var resultTask = await Task.WhenAny(hashedTasks);

            if (resultTask == timeoutTask)
                throw new TimeoutException();

            if (resultTask.IsCompletedSuccessfully)
                return resultTask;

            hashedTasks.Remove(resultTask);
        }

        return Task.FromException<T>(new InvalidOperationException("All tasks is faulted"));
    }

    public static async Task<T> CreateTaskDelayWithResult<T>(TimeSpan timeout, T result = default)
    {
        await Task.Delay(timeout);
        return result;
    }
}