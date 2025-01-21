using System;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRWLock: ISharedResource
{
    private string _data;
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    public void Write(string data)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _data = data;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public string Read()
    {
        _rwLock.EnterReadLock();
        try
        {
            return _data;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }
    
    public long ComputeFactorial(int number)
    {
        _rwLock.EnterReadLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    private long Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}