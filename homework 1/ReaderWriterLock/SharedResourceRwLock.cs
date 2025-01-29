namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly object lockObject = new();
    private string resource = "";
    
    public override void Write(string data)
    {
        lock (lockObject) resource = data;
    }

    public override string Read()
    {
        lock (lockObject) return resource;
    }

    public override long ComputeFactorial(int number)
    {
        lock(lockObject) return Factorial(number);
    }
}