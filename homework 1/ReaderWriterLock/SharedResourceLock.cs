namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string source;
    private readonly object lockObject = new();
    
    public override void Write(string data)
    {
        lock (lockObject)
        {
            source = data;
        }
    }

    public override string Read()
    {
        lock (lockObject)
        {
            return source;
        }
    }

    public override long ComputeFactorial(int number)
    {
        lock (lockObject)
        {
            return Factorial(number);
        }
    }
}