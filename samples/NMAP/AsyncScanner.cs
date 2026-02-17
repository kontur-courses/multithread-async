using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace NMAP
{
    public class AsyncScanner : IPScanner
    {
        protected virtual ILog log => LogManager.GetLogger(typeof(AsyncScanner));
        
        public Task Scan(IPAddress[] ipAdrrs, int[] ports)
        {
            throw new System.NotImplementedException();
        }
    }
}