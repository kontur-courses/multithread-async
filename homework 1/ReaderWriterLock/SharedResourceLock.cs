namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string _data;
    private readonly object _lock = new();

    public override void Write(string data)
    {
        lock (_lock)
        {
            _data = data;
        }
    }

    public override string Read()
    {
        lock (_lock)
        {
            return _data;
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