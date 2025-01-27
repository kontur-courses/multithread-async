using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string _sharedResource = string.Empty;
    private ReaderWriterLockSlim _rwLock = new();
    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        _sharedResource = data;
        _rwLock.ExitWriteLock();
    }

    public override string Read()
    {
        try
        {
            _rwLock.EnterReadLock();
            return _sharedResource;
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