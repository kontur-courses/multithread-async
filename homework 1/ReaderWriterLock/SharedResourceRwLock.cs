using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string _source;
    private readonly ReaderWriterLockSlim _locker = new();

    public override void Write(string data)
    {
        _locker.EnterWriteLock();
        try
        {
            _source = data;
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    public override string Read()
    {
        string result;
        _locker.EnterReadLock();
        try
        {
            result = _source;
        }
        finally
        {
            _locker.ExitReadLock();
        }
        return result;
    }

    public override long ComputeFactorial(int number)
    {
        long result;
        _locker.EnterReadLock();
        try
        {
            result = Factorial(number);        
        }
        finally
        {
            _locker.ExitReadLock();
        }
        return result;
    }
}