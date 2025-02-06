using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string value;
    private readonly ReaderWriterLockSlim locker = new();
    
    public override void Write(string data)
    {
        locker.EnterWriteLock();
        try
        {
            value = data;
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public override string Read()
    {
        locker.EnterReadLock();
        var result = value;
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