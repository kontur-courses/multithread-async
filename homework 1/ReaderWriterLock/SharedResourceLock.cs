namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly object _lockObject = new();
    private string _sharedSource = string.Empty;
    public override void Write(string data)
    {
        lock (_lockObject)
        {
            _sharedSource = data;
        }
    }

    public override string Read()
    {
        lock (_lockObject)
        {
            return _sharedSource;
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