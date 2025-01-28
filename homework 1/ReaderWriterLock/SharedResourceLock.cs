namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string _localData = "";
    private readonly object _lock = new();

    public override void Write(string data)
    {
        lock (_lock)
        {
            _localData = data;
        }
    }

    public override string Read()
    {
        lock (_lock)
        {
            return _localData;
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