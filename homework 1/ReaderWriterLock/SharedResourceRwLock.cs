using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly RwLockUtil _rwLock = new();
    private string _sharedResource;
    
    public override void Write(string data)
    {
        using (_rwLock.WriteLock())
        {
            _sharedResource = data;
        }
    }

    public override string Read()
    {
        using (_rwLock.ReadLock())
        {
            return _sharedResource;
        }
    }

    public override long ComputeFactorial(int number)
    {
        using (_rwLock.ReadLock())
        {
            return Factorial(number);
        }
    }
}