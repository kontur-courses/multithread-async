using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly List<string> _data = [];
    private readonly ReaderWriterLockSlim _rwLock = new();

    ~SharedResourceRwLock()
    {
        _rwLock?.Dispose();
    }
    
    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _data.Add(data);
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
            return _data.LastOrDefault();
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