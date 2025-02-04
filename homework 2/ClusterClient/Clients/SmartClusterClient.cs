﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading;
using System.Threading.Channels;
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
            var workServers = ReplicaAddresses.Length;

            var clietTimeout = timeout / workServers;
            var timeoutForOne = clietTimeout;

            var prevTasks = Channel.CreateUnbounded<Task<string>>();
            var check = Task.Run(() => CheckCompleted(prevTasks.Reader.ReadAllAsync()));

            foreach (var address in ReplicaAddresses)
            {
                var task = Task.Run(() => ProcessRequestAsync(CreateRequest(address + $"?query={query}")));
                await Task.WhenAny(task, check, Task.Delay(clietTimeout));

                if (check.Status is TaskStatus.RanToCompletion) return check.Result;
                
                if (task.Status is TaskStatus.RanToCompletion) return task.Result;
                if (task.Status is TaskStatus.Faulted) clietTimeout += timeoutForOne / --workServers;
                else Task.Run(() => AddWhenEnd(task, prevTasks));
            }

            throw new TimeoutException();
        }

        // а вы говорили на паре, что эти алгоритмы оптимизации не нужны)) вот на основе их я это и реализовал) я про идею из IAsyncEnumerable (похожий на ParallelQeary) - а он как раз таки играет важную роль в алгоритмах. а я уже сейчас видя его впервые понимаю, как он раобтает. 
        // для точности, я про момент, когда вы объясняли про способы работы буфера в Parallel. когда у каждого ровное количество элементов хранится, а потом когда буфер заполнен выбрасыются результаты дальше. или когда все заполнятся и сразу все выбрасываются.
        // и еще упрощу, для более явной наглядности: я имею в виду и AsyncEnumerable и ParallelQeary могут ждать, когда данные к ним поступят.
        // и да я это пишу, чтоб только доказать, что разные по своей природе сущности, могут иметь общий концепт. ибо мне не понравилось, что вы сказали, что это не нужно))
        private async Task<string> CheckCompleted(IAsyncEnumerable<Task<string>> tasks)
        {
            await foreach (var task in tasks)
            {
                if (task.IsCompleted)
                {
                    return task.Result;
                }
            }

            return null;
        }

        private async Task AddWhenEnd(Task<string> task, Channel<Task<string>> channel)
        {
            await Task.WhenAll(task);
            await channel.Writer.WriteAsync(task);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}