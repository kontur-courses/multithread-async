using System;

namespace ReaderWriterLock;

public class SharedResource : ISharedResource
{
    private string _data;
    private readonly object _lock = new object();

    public void Write(string data)
    {
        lock (_lock)
        {
            _data = data;
        }
    }

    public string Read()
    {
        lock (_lock)
        {
            return _data;
        }
    }
    
    public long ComputeFactorial(int number)
    {
        lock (_lock)
        {
            return Factorial(number);
        }
    }

    private long Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}