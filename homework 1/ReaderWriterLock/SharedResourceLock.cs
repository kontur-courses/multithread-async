namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private object _locker = new object();
    private string _data;
    public override void Write(string data)
    {
        lock (_locker)
        {
            _data = data;
        }
    }

    public override string Read()
    {
        lock (_locker)
        {
            return _data;
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