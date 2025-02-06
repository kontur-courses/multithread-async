namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string value;
    private readonly object locker = new();
    
    public override void Write(string data)
    {
        lock (locker)
            value = data;
    }

    public override string Read()
    {
        lock (locker)
            return value;
    }

    public override long ComputeFactorial(int number)
    {
        lock (locker)
            return Factorial(number);
    }
}