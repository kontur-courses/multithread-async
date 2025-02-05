using System;
using System.Collections.Generic;

namespace ReaderWriterLock;

public abstract class SharedResourceBase
{
    protected SortedSet<string> _sharedResource = new SortedSet<string>();
    
    public abstract void Write(string data);
    public abstract string[] Read();
    public abstract long ComputeFactorialRead(int number);
    public abstract long ComputeFactorialWrite(int number);
    
    public int Count => _sharedResource.Count;
    
    protected long Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}