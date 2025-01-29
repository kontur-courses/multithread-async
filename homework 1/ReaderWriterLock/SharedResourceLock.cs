namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string _localData;
    private readonly object _locker = new();

    public override void Write(string data)
    {
        lock (_locker)
        {
            _localData = data;
        }
    }

    public override string Read()
    {
        lock (_locker)
        {
            return _localData;
        }
    }

    public override long ComputeFactorial(int number)
    {
        lock (_locker)
        {
            return Factorial(number);
        }
    }
}