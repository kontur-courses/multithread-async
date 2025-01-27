using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private readonly object _lockObj = new();
    private readonly object _factorialLocker = new();
    private int _readersCount = 0;
    private int _writersCount = 0;
    private string _data = "";

    public override void Write(string data)
    {
        lock (_lockObj)
        {
            _writersCount++;
            while (_readersCount > 0)
                Monitor.Wait(_lockObj);

            _data += data;

            _writersCount--;
            Monitor.PulseAll(_lockObj);
        }
    }

    public override string Read()
    {
        lock (_lockObj)
        {
            while (_writersCount > 0)
                Monitor.Wait(_lockObj);

            _readersCount++;
        }

        var result = _data;

        lock (_lockObj)
        {
            _readersCount--;
            if (_readersCount == 0)
                Monitor.PulseAll(_lockObj);
        }

        return result;
    }

    public override long ComputeFactorial(int number)
    {
        lock (_factorialLocker)
            return Factorial(number);
    }
}