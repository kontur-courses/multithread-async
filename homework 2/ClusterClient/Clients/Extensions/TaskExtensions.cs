using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Clients.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<string> WaitForFirstSuccessAsync(
            this List<Task<string>> tasks,
            string errorMessage,
            CancellationTokenSource cancellationTokenSource)
        {
            if (tasks.Count == 0)
            {
                throw new InvalidOperationException("The task list cannot be empty");
            }

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);

                if (completedTask.IsCompletedSuccessfully)
                {
                    await cancellationTokenSource.CancelAsync();
                    return completedTask.Result;
                }

                tasks.Remove(completedTask);
            }

            throw new TimeoutException(errorMessage);
        }
    }
}
