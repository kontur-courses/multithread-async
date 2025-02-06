using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

    public override void Write(string data)
    {
        locker.EnterWriteLock();
        try
        {
            values.Add(data);
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
            if (values.Count == 0)
            {
                return null;
            }
            return values[^1];
        }
        finally
        {
            locker.ExitReadLock();
        }
    }

    public override long ComputeFactorialRead(int number)
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

    public override long ComputeFactorialWrite(int number)
    {
        locker.EnterWriteLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }
}