using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _rwLock = new();
    private string _sharedResource;
    
    public override void Write(string data)
    {
        _rwLock.EnterWriteLock();
        _sharedResource = data;
        _rwLock.ExitWriteLock();
    }

    public override string Read()
    {
        _rwLock.EnterReadLock();
        var result = _sharedResource;
        _rwLock.ExitReadLock();
        
        return result;
    }

    public override long ComputeFactorial(int number)
    {
        _rwLock.EnterReadLock();
        var result = Factorial(number);
        _rwLock.ExitReadLock();

        return result;
    }
}