using ClusterHistory.Models;

namespace ClusterHistory.Interfaces;

public interface IReplicaSendHistory
{
    public void AddSendAttempt(string uri);

    public void AddWorkTime(string address, TimeSpan workTime);

    public IEnumerable<string> RetrieveAddressesInOrder(ICollection<string> replicaAddresses,
        SortOrder sortOrder = SortOrder.Ascending);
}