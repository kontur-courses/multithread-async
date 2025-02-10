namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string data;
    private readonly object locker = new();

    public override void Write(string data)
    {
        lock (locker)
            this.data = data;
    }

    public override string Read()
    {
        lock (locker)
            return data;
    }

    public override long ComputeFactorial(int number)
    {
        lock (locker)
            return Factorial(number);
    }
}