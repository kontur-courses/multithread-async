using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string resource = "";
    private readonly ReaderWriterLockSlim lockObject = new();
    
    public override void Write(string data)
    {
        lockObject.EnterWriteLock();
        resource = data;
        lockObject.ExitWriteLock();
    }

    public override string Read()
    {
        lockObject.EnterReadLock();
        var result = resource;
        lockObject.ExitReadLock();
        return result;
    }

    public override long ComputeFactorial(int number)
    {
        lockObject.EnterReadLock();
        var result = Factorial(number);
        lockObject.ExitReadLock();
        return result;
    }
}