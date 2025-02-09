using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var timeLimit = timeout;
            var totalCount = ReplicaAddresses.Length;
            var requestTimeout = timeout / totalCount;
            var unfinished = new List<Task<string>>();
            var timer = new Stopwatch();

            foreach (var replicaAddress in ReplicaAddresses)
            {
                var requestTask = ProcessRequestAsync(replicaAddress, query);
                unfinished.Add(requestTask);
                var task = Task.WhenAny(unfinished);
                var delayTask = Task.Delay(requestTimeout);
                timer.Restart();
                var resultTask = await Task.WhenAny(task, delayTask);
                timer.Stop();
                if (resultTask.IsFaulted)
                {
                    unfinished.Remove(task.Result);
                }
                if (resultTask != delayTask && !task.Result.IsFaulted)
                {
                    return await task.Result;
                }
                totalCount -= 1;
                timeLimit -= timer.Elapsed;
                if (timeLimit.TotalMilliseconds <= 0)
                    break;
                if (totalCount > 1)
                    requestTimeout = timeLimit / totalCount;
            }
            while (unfinished.Count > 0 && !timeoutTask.IsCompleted)
            {
                // Запускаем таску ожидания наших запросов
                var resultTaskWait = Task.WhenAny(unfinished);
                // Запускаем и ожидаем таску ожидания выполнения верхней таски, которая ожидает выполнения запросов и таски таймаута
                // (чтобы при таймауте мы почти сразу завершились, увидели, что таймаут и вышли с исключением, а не ждали пока кто-то первый свалится сам)
                var result = await Task.WhenAny(resultTaskWait, timeoutTask);
                if (result == timeoutTask) // Если результат ожидания = таске таймаута, выходим с исключением
                    throw new TimeoutException();
                // Если это результат по запросам, то смотрим, удачный ли он. Если да - делаем await его результата, если нет, удаляем из списка запросов которые выполняются и идем на следующий круг
                if (!resultTaskWait.Result.IsFaulted) return await resultTaskWait.Result;
                unfinished.Remove(resultTaskWait.Result);
            }
            throw new TimeoutException();
        }

        private async Task<string> ProcessRequestAsync(string replicaAddress, string query)
        {
            var request = CreateRequest(replicaAddress + "?query=" + query);
            Log.InfoFormat($"Processing {request.RequestUri}");
            return await ProcessRequestAsync(request);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}