using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim locker = new();
    private string source;

    public override void Write(string data)
    {
        locker.EnterWriteLock();
        try
        {
            source = data;
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public override string Read()
    {
        locker.EnterReadLock();
        try
        {
            return source;
        }
        finally
        {
            locker.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        locker.EnterReadLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            locker.ExitReadLock();
        }
    }
}