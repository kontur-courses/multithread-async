using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _locker = new();

    public override void Write(string data)
    {
        _locker.EnterWriteLock();

        try
        {
            _resource = data;
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
            return _resource;
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