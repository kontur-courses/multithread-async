using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string _localData;
    private readonly ReaderWriterLockSlim _rwLock = new();

    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _localData = data;
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
            return _localData;
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