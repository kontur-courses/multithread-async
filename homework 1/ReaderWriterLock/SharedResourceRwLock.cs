using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _lock = new();
    private string _data = string.Empty;

    public override void Write(string data)
    {
        _lock.EnterWriteLock();
        try
        {
            _data += data;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        _lock.EnterReadLock();
        try
        {
            return _data;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        _lock.EnterReadLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}