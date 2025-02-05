using System;

namespace ReaderWriterLock;

public class SharedResourceRwLock : SharedResourceBase
{
    private readonly System.Threading.ReaderWriterLockSlim @lock = new();
    private string Data;

    public override void Write(string data)
    {
        @lock.EnterWriteLock();
        try
        {
            Data = data;
        }
        finally
        {
            //Console.WriteLine($"поток на запись: записал {Data}");
            @lock.ExitWriteLock();
        }
    }

    public override string Read()
    {
        @lock.EnterReadLock();
        try
        {
            return Data;
        }
        finally
        {
            // Console.WriteLine($"поток на чтение: прочитал {Data}");
            @lock.ExitReadLock();
        }
    }

    public override long ComputeFactorial(int number)
    {
        return Factorial(number);
    }
}