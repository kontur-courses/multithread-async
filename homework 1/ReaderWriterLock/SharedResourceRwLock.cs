using System;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private ReaderWriterLockSlim _resourceLock = new ReaderWriterLockSlim();

    private string _resource = String.Empty;

    public override void Write(string data)
    {
        try
        {
            _resourceLock.EnterWriteLock();
            _resource = data;
        }
        finally
        {
            _resourceLock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        try
        {
            _resourceLock.EnterReadLock();
            return _resource;
        }
        finally
        {
            _resourceLock.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        return Factorial(number);
    }
}