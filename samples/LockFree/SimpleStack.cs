namespace LockFree
{
    public class SimpleStack<T> : IStack<T>
    {
        private readonly object syncObj = new object();
        private Node<T> head;

        public void Push(T obj)
        {
            var newHead = new Node<T> { Value = obj };

            lock (syncObj)
            {
                if (head == null)
                    head = newHead;
                else
                {
                    newHead.Next = head;
                    head = newHead;
                }
            }
        }

        public T Pop()
        {
            T value;

            lock (syncObj)
            {
                value = head.Value;
                head = head.Next;
            }

            return value;
        }
    }
}