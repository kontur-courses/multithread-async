using System;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string data;
    private readonly ReaderWriterLockSlim locker = new();

    public override void Write(string data)
    {
        locker.EnterWriteLock();
        try
        {
            this.data = data;
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
            return data;
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