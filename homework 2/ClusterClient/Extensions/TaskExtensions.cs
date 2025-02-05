using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Extensions;

public static class TaskExtensions
{
    public static async Task<Task<T>> WhenAnyFirstSuccessfulTask<T>(this IEnumerable<Task<T>> tasks, TimeSpan timeout,
        CancellationToken linkedToken = default)
    {
        var searchSuccessTask = tasks.GetFirstSuccessTask(linkedToken);
        var completedTask = await Task.WhenAny(searchSuccessTask, Task.Delay(timeout, linkedToken));
        if (completedTask != searchSuccessTask)
        {
            throw new TimeoutException("Запрос превысил время ожидания.");
        }

        var resultTask = await searchSuccessTask;
        return resultTask;
    }

    public static async Task<Task<T>> GetFirstSuccessTask<T>(this IEnumerable<Task<T>> tasks,
        CancellationToken cancellationToken = default)
    {
        var pendingTasks = new HashSet<Task<T>>(tasks);
        var exceptions = new List<Exception>();
        while (pendingTasks.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var completedTask = await Task.WhenAny(pendingTasks);
            if (completedTask.IsCompletedSuccessfully)
            {
                return completedTask;
            }

            if (completedTask.IsFaulted)
            {
                exceptions.Add(completedTask.Exception);
            }

            pendingTasks.Remove(completedTask);
        }

        throw new AggregateException("Не удалось найти успешную задачу.", exceptions);
    }
}