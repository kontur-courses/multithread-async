namespace ReaderWriterLock;

public class SharedResourceLock() : SharedResourceBase
{
    public override void Write(string data)
    {
        lock (values)
        {
            values.Add(data);
        }
    }

    public override string Read()
    {
        lock (values)
        {
            if (values.Count == 0)
            {
                return null;
            }
            return values[^1];
        }
    }

    public override long ComputeFactorialRead(int number)
    {
        lock (values)
        {
            return Factorial(number);
        }
    }

    public override long ComputeFactorialWrite(int number)
        => ComputeFactorialRead(number);
}