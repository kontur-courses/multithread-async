namespace ReaderWriterLock;

public class SharedResourceLock : SharedResourceBase
{
    public override void Write(string data)
    {
        throw new System.NotImplementedException();
    }

    public override string Read()
    {
        throw new System.NotImplementedException();
    }

    public override long ComputeFactorial(int number)
    {
        throw new System.NotImplementedException();
    }
}