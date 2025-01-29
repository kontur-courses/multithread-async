using System.Text;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private StringBuilder sharedResource = new StringBuilder();
    private ReaderWriterLockSlim lockObject = new ReaderWriterLockSlim();
    public override void Write(string data, int id, bool withLoadImitation = false)
    {
        lockObject.EnterWriteLock();
        try
        {
            if (withLoadImitation) Factorial(FactorialNumberForLoadImitation);
            sharedResource.Clear();
            sharedResource.Append(data);
            LastWriterThreadIndex = id;
        }
        finally
        {
            lockObject.ExitWriteLock();
        }
    }

    public override string Read(bool withLoadImitation = false)
    {
        lockObject.EnterReadLock();
        try
        {
            if (withLoadImitation) Factorial(FactorialNumberForLoadImitation);
            return sharedResource.ToString();
        }
        finally
        {
            lockObject.ExitReadLock();
        }
    }
}