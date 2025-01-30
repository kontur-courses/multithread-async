using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string data;
    private readonly ReaderWriterLockSlim rwLock = new();

    public override void Write(string data)
    {
        rwLock.EnterWriteLock();
        this.data = data;
        rwLock.ExitWriteLock();
    }

    public override string Read()
    {
        rwLock.EnterReadLock();
        var result = data;
        rwLock.ExitReadLock();
        
        return result;
    }

    public override long ComputeFactorial(int number)
    {
        return Factorial(number);
    }
}