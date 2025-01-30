namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    private string eternalData;
    private readonly object lockingObject = new ();
    public override void Write(string data)
    {
        lock (lockingObject){
            eternalData = data;
        }
    }

    public override string Read()
    {
        lock (lockingObject){
            return eternalData;
        }
    }

    public override long ComputeFactorial(int number)
    {
        lock (lockingObject)
        {
            return Factorial(number);
        }
    }
}