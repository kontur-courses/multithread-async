using System;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim rwLock = new ();
    private string _data;
    public override void Write(string data)
    {
        try
        {
            rwLock.EnterWriteLock();
            _data = data;
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        try
        {
            rwLock.EnterReadLock();
            return _data;
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        try
        {
            rwLock.EnterReadLock();
            return Factorial(number);
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }
}