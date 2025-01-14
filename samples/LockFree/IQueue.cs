namespace LockFree
{
    public interface IQueue<T>
    {
        void Enqueue(T obj);
        bool TryDequeue(out T result);
    }
}