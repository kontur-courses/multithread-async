using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ClusterClient;

public class ReplicaHistory
{
    private readonly int _windowSize;
    private readonly TimeSpan _initialResponseTime;
    
    private readonly ConcurrentDictionary<string, SlidingWindow> _replicaHistories;
    
    public ReplicaHistory(IEnumerable<string> replicaAddresses, int windowSize, TimeSpan initialResponseTime)
    {
        _windowSize = windowSize;
        _initialResponseTime = initialResponseTime;
        _replicaHistories = new ConcurrentDictionary<string, SlidingWindow>();

        foreach (var address in replicaAddresses)
        {
            var window = new SlidingWindow(windowSize, initialResponseTime);
            _replicaHistories.TryAdd(address, window);
        }
    }
    
    public ReplicaHistory(IEnumerable<string> replicaAddresses) : this(replicaAddresses, 5 , TimeSpan.FromMilliseconds(100))
    {
    }

    public void AddResponseTime(string replicaAddress, TimeSpan responseTime)
    {
        if (_replicaHistories.TryGetValue(replicaAddress, out var window))
        {
            window.Add(responseTime);
        }
        else
        {
            var newWindow = new SlidingWindow(_windowSize, _initialResponseTime);
            newWindow.Add(responseTime);
            _replicaHistories.TryAdd(replicaAddress, newWindow);
        }
    }

    public IReadOnlyList<TimeSpan> GetHistoryForReplica(string replicaAddress) => 
        _replicaHistories.TryGetValue(replicaAddress, out var window) 
            ? window.GetHistory() 
            : throw new ArgumentException("Replica not found");

    public string[] GetReplicasSortedBySpeed()
    {
        return _replicaHistories
            .OrderBy(kvp => kvp.Value.Average)
            .Select(kvp => kvp.Key)
            .ToArray();
    }
    
    public int Lenght => _replicaHistories.Count;
}