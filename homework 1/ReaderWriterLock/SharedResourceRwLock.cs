using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string data;
    private readonly ReaderWriterLockSlim rwLock = new();

    public override void Write(string data)
    {
        rwLock.EnterWriteLock();
        try
        {
            this.data = data;
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        rwLock.EnterReadLock();
        try
        {
            return data;
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        rwLock.EnterReadLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }
}