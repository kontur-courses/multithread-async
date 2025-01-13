namespace LockFree
{
    public interface IStack<T>
    {
        void Push(T obj);
        T Pop();
    }
}