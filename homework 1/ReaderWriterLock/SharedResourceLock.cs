using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly Lock _lock = new();

    public override void Write(string data)
    {
        lock (_lock)
        {
            SharedResource = data;
        }
    }

    public override string Read()
    {
        lock (_lock)
        {
            return SharedResource;
        }
    }

    public override long ComputeFactorial(int number)
    {
        lock (_lock)
        {
            return Factorial(number);
        }
    }
}