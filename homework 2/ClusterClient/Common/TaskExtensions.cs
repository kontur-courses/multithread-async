using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient.Common;

public static class TaskExtensions
{
	public static async Task<T> RunTaskWithTimeoutAsync<T>(this Task<T> task, CancellationToken token)
	{
		var delayTask = Task.Delay(Timeout.Infinite, token);

		if (await Task.WhenAny(task, delayTask) == task)
			return await task;

		throw new TimeoutException("Task timed out");
	}
}