namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    public override void Write(string data)
    {
        lock (Resource)
            Resource = data;
    }

    public override string Read()
    {
        lock (Resource)
            return Resource;
    }

    public override long ComputeFactorial(int number)
    {
        lock (Resource)
            return Factorial(number);
    }
}