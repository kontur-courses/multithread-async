using System.Threading;

namespace CustomThreadPool
{
    public class WorkStealingQueue<T>
    {
        private const int INITIAL_SIZE = 32;
        private T[] m_array = new T[INITIAL_SIZE];
        private int m_mask = INITIAL_SIZE - 1;
        private volatile int m_headIndex = 0;
        private volatile int m_tailIndex = 0;
        private readonly object m_foreignLock = new object();

        public bool IsEmpty => m_headIndex >= m_tailIndex;

        public int Count => m_tailIndex - m_headIndex;

        public void LocalPush(T obj)
        {
            var tail = m_tailIndex;
            if(tail < m_headIndex + m_mask)
            {
                m_array[tail & m_mask] = obj; // safe! только в этом методе пишем в m_array
                m_tailIndex = tail + 1; // safe! только local-операции меняют m_tailIndex
            }
            else
            {
                lock (m_foreignLock)
                {
                    var head = m_headIndex;
                    var count = m_tailIndex - m_headIndex;
                    if(count >= m_mask)
                    {
                        var newArray = new T[m_array.Length << 1];
                        for(var i = 0; i < m_array.Length; i++)
                        {
                            newArray[i] = m_array[(i + head) & m_mask];
                        }
                        m_array = newArray;

                        // Reset the field values, incl. the mask.
                        m_headIndex = 0;
                        m_tailIndex = tail = count;
                        m_mask = (m_mask << 1) | 1;
                    }

                    m_array[tail & m_mask] = obj;
                    m_tailIndex = tail + 1;
                }
            }
        }

        public bool LocalPop(ref T obj)
        {
            var tail = m_tailIndex;
            if(m_headIndex >= tail) // m_headIndex может действительно уехать вперед, см. TrySteal
            {
                return false;
            }

            tail -= 1;
            Interlocked.Exchange(ref m_tailIndex, tail); // Interlocked, чтобы гарантировать, что запись не произойдет позже чтения m_headIndex в следующей строчке (C# memory model)

            if(m_headIndex <= tail)
            {
                obj = m_array[tail & m_mask];
                return true;
            }
            else
            {
                lock (m_foreignLock)
                {
                    if(m_headIndex <= tail)
                    {
                        obj = m_array[tail & m_mask];
                        return true;
                    }
                    else
                    {
                        m_tailIndex = tail + 1; // проиграли гонку
                        return false;
                    }
                }
            }
        }

        public bool TrySteal(ref T obj)
        {
            var taken = false;
            try
            {
                taken = Monitor.TryEnter(m_foreignLock);
                if(taken)
                {
                    var head = m_headIndex;
                    Interlocked.Exchange(ref m_headIndex, head + 1);  // Interlocked по аналогичным причинам, что и в LocalPop

                    if(head < m_tailIndex)
                    {
                        obj = m_array[head & m_mask];
                        return true;
                    }
                    else
                    {
                        m_headIndex = head; // проиграли гонку
                        return false;
                    }
                }
            }
            finally
            {
                if(taken)
                {
                    Monitor.Exit(m_foreignLock);
                }
            }

            return false;
        }
    }
}