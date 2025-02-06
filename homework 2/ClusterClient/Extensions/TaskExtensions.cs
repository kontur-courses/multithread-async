using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterClient
{
    public static class TaskExtensions
    {
        public static async Task<T> ExecuteWithTimeoutAsync<T>(this Task<T> task, CancellationToken token)
        {
            var delayTask = Task.Delay(Timeout.Infinite, token);

            if (await Task.WhenAny(task, delayTask) == task)
                return await task;

            throw new TimeoutException();
        }
    }
}
