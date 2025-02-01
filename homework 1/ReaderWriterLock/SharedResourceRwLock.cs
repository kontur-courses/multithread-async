using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly ReaderWriterLockSlim _rwLock = new();
    private string _data = "";

    public override void Write(string data)
    {
        using (_rwLock.WriteLock())
        {
            _data += data;
        }
    }

    public override string Read()
    {
        using (_rwLock.ReadLock())
        {
            return _data;
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