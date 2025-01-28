using System;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly object _locker = new();
    private readonly object _factorialLocker = new();
    private string _sharedResource = string.Empty;

    public override void Write(string data)
    {
        lock (_locker)
        {
            _sharedResource += data;
        }
    }

    public override string Read()
    {
        string result;
        lock (_locker)
        {
            result = _sharedResource;
        }

        return result;
    }

    public override long ComputeFactorial(int number)
    {
        long result;
        lock (_factorialLocker)
        {
            result = Factorial(number);
        }

        return result;
    }
}