namespace ReaderWriterLock;

public interface ISharedResource
{
    void Write(string data);
    string Read();
    long ComputeFactorial(int number);
}