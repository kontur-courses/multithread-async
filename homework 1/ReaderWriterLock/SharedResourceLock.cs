namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string writedData;

	private readonly object dataLockObj = new();

	public override void Write(string data)
    {
        lock(dataLockObj)
        {
            writedData = data;
		}
    }

    public override string Read()
    {
        lock (dataLockObj)
        {
            return writedData;
		}
    }

	public override long ComputeFactorial(int number)
    {
        lock (dataLockObj)
        {
            return Factorial(number);
        }
    }
}