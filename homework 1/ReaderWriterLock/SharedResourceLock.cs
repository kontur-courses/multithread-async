namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string data;
    private readonly object dataLock = new();
    private readonly object factorialLock = new();

    public override void Write(string data)
    {
        lock (dataLock)
        {
            this.data = data;
        }
    }

    public override string Read()
    {
        lock (dataLock)
        {
            return data;
        }
    }

    public override long ComputeFactorial(int number)
    {
        lock (factorialLock)
        {
            return Factorial(number);
        }
    }
}