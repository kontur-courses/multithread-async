using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockFree
{
    public class ConcurrentStackWrapper<T> : IStack<T>
    {
        private readonly ConcurrentStack<T> stack = new ConcurrentStack<T>();
        public void Push(T obj)
        {
            stack.Push(obj);
        }

        public T Pop()
        {
            if(!stack.TryPop(out var result))
                throw new NullReferenceException("Stack is empty");
            return result;
        }
    }
}
