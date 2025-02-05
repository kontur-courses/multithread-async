using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly object _lock = new object();
    
    public override void Write(string data)
    {
        lock (_lock)
        {
            _sharedResource.Add(data);
        }
    }

    public override string[] Read()
    {
        lock (_lock)
        {
            return _sharedResource.ToArray();
        }
    }

    public override long ComputeFactorialRead(int number)
    {
        lock (_lock)
        {
            return Factorial(number);
        }
    }

    public override long ComputeFactorialWrite(int number)
    {
        lock (_lock)
        {
            return Factorial(number);
        }
    }
}