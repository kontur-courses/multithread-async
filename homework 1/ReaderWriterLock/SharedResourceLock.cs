namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly object _lockObject = new();
    private string _content;
    
    public override void Write(string data)
    {
        lock (_lockObject)
        {
            _content = data;
        }
    }

    public override string Read()
    {
        lock (_lockObject)
        {
            return _content;
        }
    }

    public override long ComputeFactorial(int number)
    {
        lock (_lockObject)
        {
            return Factorial(number);
        }
    }
}