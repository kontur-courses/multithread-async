using System;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string _resource = String.Empty;

    public override void Write(string data)
    {
        lock (_resource)
        {
            _resource = data;
        }
    }

    public override string Read()
    {
        lock (_resource)
        {
            return _resource;
        }
    }

    public override long ComputeFactorial(int number)
    {
        return Factorial(number);
    }
}