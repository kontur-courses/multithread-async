using ClusterClient.ReplicasPriorityManagers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static async Task<string> WaitForFirstSuccessAsync(
            this ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)> tasks,
            string errorMessage,
            CancellationTokenSource cancellationTokenSource,
            IReplicasPriorityManager replicasPriorityManager)
        {
            if (tasks.IsEmpty)
            {
                throw new InvalidOperationException("The task dictionary cannot be empty");
            }

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks.Keys);

                var taskInfo = tasks[completedTask];
                var workingTime = taskInfo.stopwatch.Elapsed;

                if (completedTask.IsFaulted)
                {
                    replicasPriorityManager.SetReplicaStatsTime(taskInfo.address, TimeSpan.MaxValue);
                    tasks.TryRemove(completedTask, out _);
                    continue;
                }

                if (completedTask.IsCompletedSuccessfully)
                {
                    await cancellationTokenSource.CancelAsync();
                    tasks.AddInformationAboutEachTask(replicasPriorityManager);
                    return completedTask.Result;
                }
                replicasPriorityManager.AddToReplicaStatsTime(taskInfo.address, workingTime);

                tasks.TryRemove(completedTask, out var _);
            }

            throw new TimeoutException(errorMessage);
        }

        public static void AddInformationAboutEachTask(
            this ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)> tasks,
            IReplicasPriorityManager replicasPriorityManager)
        {
            foreach (var pair in tasks)
            {
                replicasPriorityManager
                    .AddToReplicaStatsTime(pair.Value.address, pair.Value.stopwatch.Elapsed);
            }
        }
    }
}
