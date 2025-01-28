using System.Collections.Generic;
using System.Text;

namespace ReaderWriterLock;

public abstract class SharedResourceBase
{
    public abstract void Write(string data);
    public abstract string Read();
    public abstract long ComputeFactorial(int number);
    protected readonly StringBuilder Resource = new();
    
    protected long Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}