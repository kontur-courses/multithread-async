using System.Collections.Generic;
using System.Linq;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly List<string> _data = [];
    private readonly object _lockObject = new();
    
    public override void Write(string data)
    {
        lock (_lockObject)
        {
            _data.Add(data);
        }
    }

    public override string Read()
    {
        lock (_lockObject)
        {
            return _data.LastOrDefault();
        }
    }

    public override long ComputeFactorial(int number)
    {
        lock (_lockObject)
        {
            return Factorial(number);
        }
    }
}