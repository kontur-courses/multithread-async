using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string data;
    private readonly ReaderWriterLockSlim rwLock = new();

    public override void Write(string data)
    {
        rwLock.EnterUpgradeableReadLock();
        try
        {
            rwLock.EnterWriteLock();
            try
            {
                this.data = data;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        finally
        {
            rwLock.ExitUpgradeableReadLock();
        }
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