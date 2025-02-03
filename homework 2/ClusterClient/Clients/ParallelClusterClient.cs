using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    
    public class ParallelClusterClient : ClusterClientBase
    {
    
        public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses
                .Select(address => Task.Run(() =>
                    ProcessRequestAsync(CreateRequest(address + $"?query={query}"), timeout)))
                .ToList();


            while (tasks.Count != 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                if (completedTask is { Status: TaskStatus.RanToCompletion }) return completedTask.Result;
            }

            throw new TimeoutException();
        }

        private async Task<string> ProcessRequestAsync(WebRequest webRequest, TimeSpan timeSpan)
        {
            var task = ProcessRequestAsync(webRequest);

            var completedTask = await Task.WhenAny(task, Task.Delay(timeSpan));

            if (task != completedTask)
            {
                // зачем возвращать таску с ошибкой, если этот метод сам по себе таска. и если сюда попадёт, то он как раз таки и вернётся себя с ошибкой
                throw new TimeoutException();
            }


            return task.Result;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}