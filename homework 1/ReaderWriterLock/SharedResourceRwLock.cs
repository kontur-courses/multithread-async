namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private string _localData = "";
    private readonly RwLockSugar _lock = new();
    
    public override void Write(string data)
    {
        using (_lock.WriteLock())
        {
            _localData = data;
        }
    }

    public override string Read()
    {
        using (_lock.ReadLock())
        {
            return _localData;
        }
    }

    public override long ComputeFactorial(int number)
    {
        using (_lock.ReadLock())
        {
            return Factorial(number);
        }
    }
}