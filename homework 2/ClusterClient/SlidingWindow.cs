using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace ClusterClient;

public class SlidingWindow
{
    private readonly long[] _buffer;
    private readonly int _capacity;
    private int _currentIndex = -1; 
    private long _totalTicks;
    
    public SlidingWindow(int capacity, TimeSpan initialValue)
    {
        _capacity = capacity;
        _buffer = new long[capacity];
        _totalTicks += initialValue.Ticks * capacity;
        for (var i = 0; i < capacity; i++)
        {
            _buffer[i] = initialValue.Ticks;
        }
    }

    public void Add(TimeSpan responseTime)
    {
        var ticks = responseTime.Ticks;
        var index = Interlocked.Increment(ref _currentIndex);
        var pos = index % _capacity;
        
        var original = Interlocked.Exchange(ref _buffer[pos], ticks);
        Interlocked.Add(ref _totalTicks, ticks - original);
    }

    public IReadOnlyList<TimeSpan> GetHistory()
    {
        var snapshot = new TimeSpan[_capacity];
        for (var i = 0; i < _capacity; i++)
        {
            snapshot[i] = TimeSpan.FromTicks(Volatile.Read(ref _buffer[i]));
        }
        return snapshot;
    }

    public long Average => _totalTicks / _capacity;
}