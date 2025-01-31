using System;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _rwLock = new();
    
    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        try
        {
            Resource = data;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        _rwLock.EnterReadLock();
        string result;
        try
        {
            result = Resource;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
        
        return result;
    }

    public override long ComputeFactorial(int number)
    {
        _rwLock.EnterReadLock();
        long result;
        try
        {
            result = Factorial(number);
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
        
        return result;
    }
}