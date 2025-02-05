namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string _sharedResource;
    private readonly object _lockObject = new(); 
    public override void Write(string data)
    {
        lock (_lockObject)
        {
            _sharedResource = data;
        }
    }

    public override string Read()
    {
        lock (_lockObject)
        {
            return _sharedResource;
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