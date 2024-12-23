using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TPL
{
    public static class TcpClientExtensions
    {
        public static PortStatus Connect(this TcpClient tcpClient, IPAddress ipAddr, int port, int timeout = 3000)
        {
            var connectTask = tcpClient.ConnectAsync(ipAddr, port);
            
            Task.WaitAny(connectTask, Task.Delay(timeout));
            
            return DecodePortStatus(connectTask);
        }

		public static async Task<PortStatus> ConnectAsync(this TcpClient tcpClient, IPAddress ipAddr, int port, int timeout = 3000)
        {
            var connectTask = tcpClient.ConnectAsync(ipAddr, port);
            
            await Task.WhenAny(connectTask, Task.Delay(timeout));
            
            return DecodePortStatus(connectTask);
        }

        private static PortStatus DecodePortStatus(Task connectTask)
        {
            return connectTask.Status switch
            {
                TaskStatus.RanToCompletion => PortStatus.Open,
                TaskStatus.Faulted => PortStatus.Closed,
                _ => PortStatus.Filtered
            };
        }
    }
}