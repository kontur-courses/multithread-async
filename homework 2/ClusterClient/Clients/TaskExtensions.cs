using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Clients;

public static class TaskExtensions
{
    public static async Task<T> RunTaskWithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var delayTask = Task.Delay(Timeout.Infinite, cancellationToken);
        var completedTask = await Task.WhenAny(task, delayTask);
        if (completedTask == delayTask) throw new TimeoutException();
        return await task;
    }
}