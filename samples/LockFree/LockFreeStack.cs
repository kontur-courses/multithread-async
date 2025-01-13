using System;
using System.Threading;

namespace LockFree
{
    public class LockFreeStack<T> : IStack<T>
    {
        private Node<T> head;

        public void Push(T obj)
        {
           throw new NotImplementedException();
        }

        public T Pop()
        {
            throw new NotImplementedException();
        }
    }
}