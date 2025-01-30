using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    public override void Write(string data)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            SharedResource = data;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        _readerWriterLock.EnterReadLock();
        try
        {
           return SharedResource;
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }
}