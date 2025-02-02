using System.Collections.Concurrent;
using ClusterHistory.Interfaces;
using ClusterHistory.Models;

namespace ClusterHistory.Implementations;

public sealed class ReplicaSendHistory : IReplicaSendHistory
{
    private readonly ConcurrentDictionary<string, ReplicaSendHistoryItem> items = new();

    internal ReplicaSendHistoryItem? GetHistoryItemOrNull(string address)
    {
        return items.GetValueOrDefault(address);
    }

    public void AddSendAttempt(string uri)
    {
        var historyItem = items.GetValueOrDefault(uri);
        if (historyItem is null)
        {
            historyItem = new ReplicaSendHistoryItem(uri);
            items.TryAdd(uri, historyItem);
        }

        historyItem.IncrementSendCount();
    }

    public void AddWorkTime(string address, TimeSpan workTime)
    {
        var historyItem = items.GetValueOrDefault(address);
        if (historyItem is null)
        {
            throw new KeyNotFoundException(
                $"Не удалось найти по этому uri: {address}. Добавьте попытку перед тем как добавлять время");
        }

        historyItem.IncrementSuccessfulSend(workTime);
    }

    public IEnumerable<string> RetrieveAddressesInOrder(ICollection<string> replicaAddresses,
        SortOrder sortOrder = SortOrder.Ascending)
    {
        AddAddressesIfNotContains(replicaAddresses);
        
        var searchItems = items.Where(r => replicaAddresses.Contains(r.Key)).ToList();
        if (searchItems.All(x => x.Value.SendWithWorkTimeCount == 0))
        {
            return replicaAddresses;
        }

        var orderedItems = searchItems
            .OrderBy(item => item.Value.SendCount == 0 ? 0 : 1)
            .ThenBy(item => item.Value.SendWithWorkTimeCount != 0 ? 0 : 1)
            .ThenBy(item => item.Value.CalculateAverageResponseTime())
            .ThenBy(item => item.Key);

        var a = orderedItems.ToList();
        
        return sortOrder == SortOrder.Descending
            ? orderedItems.Reverse().Select(x => x.Key)
            : orderedItems.Select(x => x.Key);
    }

    public void AddAddressesIfNotContains(IEnumerable<string> addresses)
    {
        foreach (var address in addresses)
        {
            items.TryAdd(address, new ReplicaSendHistoryItem(address));
        }
    }
}