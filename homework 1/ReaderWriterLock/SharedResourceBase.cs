using System.Collections.Generic;

namespace ReaderWriterLock;

public abstract class SharedResourceBase
{
    protected readonly List<string> values = new List<string>();
    public string[] Values => values.ToArray();

    public abstract void Write(string data);
    public abstract string Read();
    public abstract long ComputeFactorialRead(int number);
    public abstract long ComputeFactorialWrite(int number);

    protected long Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}