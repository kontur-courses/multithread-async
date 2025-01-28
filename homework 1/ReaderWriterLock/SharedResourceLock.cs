namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly object _lock = new();
    private string _innerData = string.Empty;
    
    public override void Write(string data)
    {
        lock (_lock)
            _innerData = data;
    }

    public override string Read()
    {
        lock (_lock)
            return _innerData;
    }

    public override long ComputeFactorial(int number)
    {
        lock (_lock)
            return Factorial(number);
    }
}