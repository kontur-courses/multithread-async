using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private ReaderWriterLockSlim _locker = new();

    public override void Write(string data)
    {
        _locker.EnterWriteLock();
        _resource = data;
        _locker.ExitWriteLock();
    }

    public override string Read()
    {
        _locker.EnterReadLock();
        var data = _resource;
        _locker.ExitReadLock();
        return data;
    }

    public override long ComputeFactorial(int number)
    {
        _locker.EnterReadLock();
        var factorial = Factorial(number);
        _locker.ExitReadLock();
        return factorial;
    }
}