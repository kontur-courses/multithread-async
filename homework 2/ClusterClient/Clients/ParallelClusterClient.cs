﻿using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = replicaAddresses
                .Select(uri => CreateRequest(uri + "?query=" + query))
                .Select(req => Task.Run(() => SilentProcessRequestAsync(req)))
                .ToList();
            
            var delayTask = Task.Delay(timeout);

            while (tasks.Count != 0)
            {
                var processTask = Task.WhenAny(tasks);
                await Task.WhenAny(processTask, delayTask);
                if (delayTask.IsCompleted) throw new TimeoutException();
                
                if (processTask.Result.Result is not null)
                    return processTask.Result.Result;
                tasks.Remove(processTask.Result);
            }
            return null;
        }
        
        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
