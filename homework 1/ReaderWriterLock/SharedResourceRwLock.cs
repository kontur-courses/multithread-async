using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim locker = new();
    private string source;

    public override void Write(string data)
    {
        locker.EnterWriteLock();
        source = data;
        locker.ExitWriteLock();
    }

    public override string Read()
    {
        locker.EnterReadLock();
        var result = source;
        locker.ExitReadLock();
        return result;
    }

    public override long ComputeFactorial(int number)
    {
        locker.EnterReadLock();
        var result = Factorial(number);
        locker.ExitReadLock();
        return result;
    }
}