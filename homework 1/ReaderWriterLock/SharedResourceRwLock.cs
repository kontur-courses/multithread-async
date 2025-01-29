using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string _localData;
    private readonly ReaderWriterLockSlim _locker = new();
    
    public override void Write(string data)
    {
        _locker.EnterWriteLock();
        _localData = data;
        _locker.ExitWriteLock();
    }

    public override string Read()
    {
        _locker.EnterReadLock();
        var result = _localData;
        _locker.ExitReadLock();
        return result;
    }

    public override long ComputeFactorial(int number)
    {
        _locker.EnterReadLock();
        var result = Factorial(number);
        _locker.ExitReadLock();
        return result;
    }
}