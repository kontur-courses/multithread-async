namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private object _locker = new();

    public override void Write(string data)
    {
        lock (_locker)
        {
            _resource = data;
        }
    }

    public override string Read()
    {
        lock (_locker)
        {
            return _resource;
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