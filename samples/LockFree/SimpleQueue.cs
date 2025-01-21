using System.Collections.Generic;
using System.Linq;

namespace LockFree
{
    public class SimpleQueue<T> : IQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
 
        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
        }

        public bool TryDequeue(out T result)
        {
            if (!queue.Any())
            {
                result = default(T);
                return false;
            }

            result = queue.Dequeue();
            return true;
        }
    }
}