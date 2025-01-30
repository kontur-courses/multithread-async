using System;
using System.Linq;
using System.Threading;

namespace ReaderWriterLock;

public class IdenticalThreadsStarter
{
    public CountdownEvent Start(Action<int> action, int count)
    {
        var countdown = new CountdownEvent(count);
        Enumerable
            .Range(0, count)
            .Select(i => new Thread(() =>
                {
                    action(i);
                    countdown.Signal();
                }))
            .ForEach(thread => thread.Start());
        return countdown;
    }
}
