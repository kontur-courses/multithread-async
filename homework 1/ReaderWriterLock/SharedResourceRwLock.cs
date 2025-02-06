using System.Threading;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
	private string writedData;

	private readonly ReaderWriterLockSlim dataLocker = new();

	public override void Write(string data)
    {
		dataLocker.EnterWriteLock();
		writedData = data;
		dataLocker.ExitWriteLock();
	}

    public override string Read()
    {
        dataLocker.EnterReadLock();
		var result = writedData;
		dataLocker.ExitReadLock();
		
		return result;
    }

	public override long ComputeFactorial(int number)
	{
		dataLocker.EnterReadLock();
		var result = Factorial(number);
		dataLocker.ExitReadLock();

		return result;
	}
}