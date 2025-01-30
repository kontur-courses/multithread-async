namespace ReaderWriterLock;

public abstract class SharedResourceBase
{
    protected string? _resource;

    public abstract void Write(string data);
    public abstract string Read();
    public abstract long ComputeFactorial(int number);

    protected long Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}