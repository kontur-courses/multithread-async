using System.Text;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private StringBuilder sharedResource = new StringBuilder();
    private object lockObject = new object();
    public int _threadID = -1;

    public override void Write(string data, int id,  bool withLoadImitation = false)
    {
        lock (lockObject)
        {
            if (withLoadImitation)
                Factorial(FactorialNumberForLoadImitation);

            //StringBuilder в качестве ресурса взят для того, чтобы возникали проблемы при отсутствии синхронизации
            sharedResource.Clear();
            sharedResource.Append(data);
            LastWriterThreadIndex = id;
        }
    }

    public override string Read(bool withLoadImitation = false)
    {
        lock (lockObject)
        {
            if (withLoadImitation) Factorial(FactorialNumberForLoadImitation);
            return sharedResource.ToString();
        }
    }
}