using System;
using System.Collections.Generic;
using System.Linq;
using ClusterHistory.Implementations;
using ClusterHistory.Models;
using Fclp.Internals.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterTests;

[TestFixture]
[TestOf(typeof(ReplicaSendHistory))]
public class ReplicaSendHistoryTests
{
    [Test]
    public void GetHistoryItemOrNull_ShouldReturnsHistoryItem_WhenAddressExistsInHistory()
    {
        var history = new ReplicaSendHistory();
        history.AddSendAttempt("http://example.com/replica1");

        var item = history.GetHistoryItemOrNull("http://example.com/replica1");

        item.Should().NotBeNull();
    }

    [Test]
    public void GetHistoryItemOrNull_ShouldReturnsNull_WhenAddressDoesNotExistInHistory()
    {
        var history = new ReplicaSendHistory();

        var item = history.GetHistoryItemOrNull("http://example.com/replica1");

        item.Should().BeNull();
    }

    [Test]
    public void AddSendAttempt_ShouldIncrementsSendCount_WhenCalledMultipleTimesForSameAddress()
    {
        var history = new ReplicaSendHistory();
        const string address = "http://example.com/replica1";
        var replicaSendHistoryItem = new ReplicaSendHistoryItem(address);
        replicaSendHistoryItem.IncrementSendCount();
        replicaSendHistoryItem.IncrementSendCount();

        history.AddSendAttempt(address);
        history.AddSendAttempt(address);

        var item = history.GetHistoryItemOrNull(address);
        item.Should().BeEquivalentTo(replicaSendHistoryItem);
    }

    [Test]
    public void AddSendAttempt_ShouldSetsSendCountToOne_WhenCalledOnceForAddress()
    {
        var history = new ReplicaSendHistory();
        const string address = "http://example.com/replica1";
        var replicaSendHistoryItem = new ReplicaSendHistoryItem(address);
        replicaSendHistoryItem.IncrementSendCount();

        history.AddSendAttempt(address);

        var item = history.GetHistoryItemOrNull(address);
        item.Should().BeEquivalentTo(replicaSendHistoryItem);
    }

    [Test]
    public void AddWorkTime_ShouldIncrementsSuccessfulSendCountAndAddsWorkTime_WhenAddressExists()
    {
        var history = new ReplicaSendHistory();
        var workTime = TimeSpan.FromMilliseconds(100);
        const string address = "http://example.com/replica1";
        var replicaSendHistoryItem = new ReplicaSendHistoryItem(address);
        replicaSendHistoryItem.IncrementSendCount();
        replicaSendHistoryItem.IncrementSuccessfulSend(workTime);

        history.AddSendAttempt(address);
        history.AddWorkTime(address, workTime);

        var item = history.GetHistoryItemOrNull(address);
        item.Should().BeEquivalentTo(replicaSendHistoryItem);
    }

    [Test]
    public void AddWorkTime_ShouldAccumulatesSuccessfulSendCountAndAddsWorkTime_WhenCalledMultipleTimesForSameAddress()
    {
        var history = new ReplicaSendHistory();
        var workTime1 = TimeSpan.FromMilliseconds(100);
        var workTime2 = TimeSpan.FromMilliseconds(200);
        const string address = "http://example.com/replica1";
        var replicaSendHistoryItem = new ReplicaSendHistoryItem(address);
        replicaSendHistoryItem.IncrementSendCount();
        replicaSendHistoryItem.IncrementSuccessfulSend(workTime1);
        replicaSendHistoryItem.IncrementSuccessfulSend(workTime2);

        history.AddSendAttempt(address);
        history.AddWorkTime(address, workTime1);
        history.AddWorkTime(address, workTime2);

        var item = history.GetHistoryItemOrNull(address);
        item.Should().BeEquivalentTo(replicaSendHistoryItem);
    }

    [Test]
    public void AddWorkTime_ShouldThrowsKeyNotFoundException_WhenAddressDoesNotExist()
    {
        var history = new ReplicaSendHistory();
        const string address = "http://example.com/replica1";
        var workTime = TimeSpan.FromMilliseconds(100);

        var act = () => history.AddWorkTime(address, workTime);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"Не удалось найти по этому uri: {address}. Добавьте попытку перед тем как добавлять время");
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldContainsAllAddresses_WhenOrderNewAddresses()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica2", "http://example.com/replica3" };

        history.RetrieveAddressesInOrder(addresses);

        var items = addresses.Select(x => history.GetHistoryItemOrNull(x)).Where(x => x is not null);
        items.Count().Should().Be(addresses.Count);
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldDoesNotAddNewEntry_WhenAddressAlreadyExists()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string> { "http://example.com/replica1", "http://example.com/replica2" };

        history.AddSendAttempt(addresses.First());
        history.RetrieveAddressesInOrder(addresses);

        var items = addresses.Select(x => history.GetHistoryItemOrNull(x)).Where(x => x is not null);
        items.Count().Should().Be(addresses.Count);
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldPrioritizesAddressesWithZeroSendCountAndLowerAverageTime_WhenSomeAddressesHaveNoSuccessfulSends()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica2", "http://example.com/replica3" };
        var expectedAddresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica3", "http://example.com/replica2" };
        addresses.Skip(1).ForEach(x => history.AddSendAttempt(x));
        history.AddWorkTime(addresses[1], TimeSpan.FromMilliseconds(200));
        history.AddWorkTime(addresses[2], TimeSpan.FromMilliseconds(100));
        history.AddSendAttempt(addresses[2]);

        var orderAddresses = history.RetrieveAddressesInOrder(addresses);

        orderAddresses.Should().ContainInOrder(expectedAddresses);
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldPrioritizesAddressesWithLowerAverageTime_WhenAllAddressesHaveSuccessfulSends()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica2", "http://example.com/replica3" };
        var expectedAddresses = new List<string>
            { "http://example.com/replica3", "http://example.com/replica2", "http://example.com/replica1" };
        addresses.ForEach(x => history.AddSendAttempt(x));
        history.AddWorkTime(addresses[1], TimeSpan.FromMilliseconds(200));
        history.AddWorkTime(addresses[2], TimeSpan.FromMilliseconds(100));
        history.AddSendAttempt(addresses[2]);

        var orderAddresses = history.RetrieveAddressesInOrder(addresses);

        orderAddresses.Should().ContainInOrder(expectedAddresses);
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldPrioritizesAddressesByLowerAverageSendTime_WhenAddressesHaveManySuccessfulSends()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica2", "http://example.com/replica3" };
        var expectedAddresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica3", "http://example.com/replica2" };
        addresses.ForEach(x => history.AddSendAttempt(x));
        history.AddWorkTime(addresses[0], TimeSpan.FromMilliseconds(50));
        history.AddWorkTime(addresses[1], TimeSpan.FromMilliseconds(200));
        history.AddWorkTime(addresses[2], TimeSpan.FromMilliseconds(150));
        addresses.ForEach(x => history.AddSendAttempt(x));
        history.AddWorkTime(addresses[0], TimeSpan.FromMilliseconds(100));
        history.AddWorkTime(addresses[1], TimeSpan.FromMilliseconds(300));
        history.AddWorkTime(addresses[2], TimeSpan.FromMilliseconds(150));

        var orderAddresses = history.RetrieveAddressesInOrder(addresses);

        orderAddresses.Should().ContainInOrder(expectedAddresses);
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldEmptyCollection_WhenInputCollectionIsEmpty()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string>();

        var orderAddresses = history.RetrieveAddressesInOrder(addresses);

        orderAddresses.Should().BeEmpty();
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldSortByName_WhenEqualPerformance()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string>
            { "http://example.com/replica3", "http://example.com/replica2", "http://example.com/replica1" };
        var expectedAddresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica2", "http://example.com/replica3" };
        addresses.ForEach(x => history.AddSendAttempt(x));
        history.AddWorkTime(addresses[0], TimeSpan.FromMilliseconds(100));
        history.AddWorkTime(addresses[1], TimeSpan.FromMilliseconds(100));
        history.AddWorkTime(addresses[2], TimeSpan.FromMilliseconds(100));

        var orderAddresses = history.RetrieveAddressesInOrder(addresses);

        orderAddresses.Should().ContainInOrder(expectedAddresses);
    }

    [Test]
    public void RetrieveAddressesInOrder_ShouldDescendingOrder_WhenDescendingSortOrderIsSpecified()
    {
        var history = new ReplicaSendHistory();
        var addresses = new List<string>
            { "http://example.com/replica1", "http://example.com/replica2", "http://example.com/replica3" };
        var expectedAddresses = new List<string>
            { "http://example.com/replica3", "http://example.com/replica2", "http://example.com/replica1" };
        addresses.ForEach(x => history.AddSendAttempt(x));
        history.AddWorkTime(addresses[0], TimeSpan.FromMilliseconds(50));
        history.AddWorkTime(addresses[1], TimeSpan.FromMilliseconds(100));
        history.AddWorkTime(addresses[2], TimeSpan.FromMilliseconds(150));

        var orderAddresses = history.RetrieveAddressesInOrder(addresses, SortOrder.Descending);

        orderAddresses.Should().ContainInOrder(expectedAddresses);
    }
}