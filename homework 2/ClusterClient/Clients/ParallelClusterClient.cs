﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient(string[] replicaAddresses) : ClusterClientBase(replicaAddresses)
    {
        public override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
