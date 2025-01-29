using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _lockSlim = new();
    private string _content;
    
    public override void Write(string data)
    {
        _lockSlim.EnterWriteLock();
        _content = data;
        _lockSlim.ExitWriteLock();
    }

    public override string Read()
    {
        _lockSlim.EnterReadLock();
        
        var readContent = _content;
        
        _lockSlim.ExitReadLock();

        return readContent;
    }

    public override long ComputeFactorial(int number)
    {
        _lockSlim.EnterReadLock();
        
        var factorial = Factorial(number);
        
        _lockSlim.ExitReadLock();

        return factorial;
    }
}