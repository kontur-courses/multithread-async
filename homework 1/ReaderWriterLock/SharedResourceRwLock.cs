using System;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _rwLock = new();
    private string _sharedResource = string.Empty;

    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _sharedResource += data;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        string result;
        _rwLock.EnterReadLock();
        try
        {
            result = _sharedResource;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }

        return result;
    }

    public override long ComputeFactorial(int number)
    {
        long result;
        _rwLock.EnterReadLock();
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