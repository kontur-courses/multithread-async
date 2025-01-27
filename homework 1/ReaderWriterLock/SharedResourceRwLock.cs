using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _rwLock = new();
    private string _innerData = string.Empty;
    
    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _innerData = data;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        _rwLock.EnterReadLock();
        try
        {
            return _innerData;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
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
}