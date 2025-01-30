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
        eternalData += data;
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
        lockingObject.EnterReadLock();
        var outputString = Factorial(number);
        lockingObject.ExitReadLock();
        return outputString;
    }
}