namespace ReaderWriterLock;

public abstract class SharedResourceBase
{
    public abstract void Write(string data, int threadId = 0, bool withLoadImitation = false);
    public abstract string Read(bool withLoadImitation = false);
    public int LastWriterThreadIndex { get; protected set; }
    public int FactorialNumberForLoadImitation { get; set; } = 1;
    protected long Factorial(long number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}