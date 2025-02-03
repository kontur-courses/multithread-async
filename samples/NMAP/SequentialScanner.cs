using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace NMAP
{
    public class SequentialScanner : IPScanner
    {
        protected virtual ILog log => LogManager.GetLogger(typeof(SequentialScanner));

        public virtual Task Scan(IPAddress[] ipAddrs, int[] ports)
        {
            return Parallel.ForEachAsync(ipAddrs, new ParallelOptions() { MaxDegreeOfParallelism = 10 },
                async (address, token) =>
                {
                    if (await PingAddr(address) != IPStatus.Success) return;

                    await Task.WhenAll(ports.Select(x => CheckPort(address, x)));
                });
        }

        protected async Task<IPStatus> PingAddr(IPAddress ipAddr, int timeout = 3000)
        {
            log.Info($"Pinging {ipAddr}");
            using var ping = new Ping();
            var status = await ping.SendPingAsync(ipAddr, timeout);
            log.Info($"Pinged {ipAddr}: {status}");
            return status.Status;
        }

        protected async Task CheckPort(IPAddress ipAddr, int port, int timeout = 3000)
        {
            using var tcpClient = new TcpClient();
            log.Info($"Checking {ipAddr}:{port}");

            var connectTask = await tcpClient.ConnectWithTimeoutAsync(ipAddr, port, timeout);
            var portStatus = connectTask.Status switch
            {
                TaskStatus.RanToCompletion => PortStatus.OPEN,
                TaskStatus.Faulted => PortStatus.CLOSED,
                _ => PortStatus.FILTERED
            };

            log.Info($"Checked {ipAddr}:{port} - {portStatus}");
        }
    }
}