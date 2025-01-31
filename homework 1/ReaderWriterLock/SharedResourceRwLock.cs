using System;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string _localData;
    private readonly ReaderWriterLockSlim _locker = new();
    
    public override void Write(string data)
    {
        _locker.EnterWriteLock();
        try
        {
            _localData = data;
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    public override string Read()
    {
        _locker.EnterReadLock();
        try
        {
            return _localData;
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        _locker.EnterReadLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }
}