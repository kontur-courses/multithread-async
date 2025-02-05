using System.Linq;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    ~SharedResourceRwLock()
    {
        _rwLock.Dispose();
    }
    
    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _sharedResource.Add(data);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public override string[] Read()
    {
        _rwLock.EnterReadLock();
        try
        {
            return _sharedResource.ToArray();
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public override long ComputeFactorialRead(int number)
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

    public override long ComputeFactorialWrite(int number)
    {
        _rwLock.EnterWriteLock();
        try
        {
            return Factorial(number);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }
}