using System.Data.Common;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim lockingObject = new();
    private string eternalData;
    public override void Write(string data)
    {
        lockingObject.EnterWriteLock();
        eternalData = data;
        lockingObject.ExitWriteLock();
    }

    public override string Read()
    {
        lockingObject.EnterReadLock();
        var outputString = eternalData;
        lockingObject.ExitReadLock();
        return outputString;
    }

    public override long ComputeFactorial(int number)
    {
        long outputString = -1;
        lockingObject.EnterReadLock();
        try
        {
            outputString = Factorial(number);
        }
        finally
        {
            lockingObject.ExitReadLock();
        }
        return outputString;
    }
}