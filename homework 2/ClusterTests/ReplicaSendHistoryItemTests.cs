using System;
using System.Linq;
using System.Threading;
using ClusterHistory.Models;
using FluentAssertions;
using NUnit.Framework;

namespace ClusterTests;

[TestFixture]
[TestOf(typeof(ReplicaSendHistoryItem))]
public class ReplicaSendHistoryItemTests
{
    [Test]
    public void IncrementSendCount_ShouldIncrementsSendCountCorrectly()
    {
        const string replica = "http://example.com/replica";
        var item = new ReplicaSendHistoryItem(replica);

        item.IncrementSendCount();
        item.IncrementSendCount();

        item.SendCount.Should().Be(2);
    }

    [Test]
    public void CalculateAverageResponseTime_ShouldReturnsZero_WhenNoWorkTimeHasBeenAdded()
    {
        const string replica = "http://example.com/replica";
        var item = new ReplicaSendHistoryItem(replica);

        var averageTime = item.CalculateAverageResponseTime();

        averageTime.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void CalculateAverageResponseTime_ShouldCalculatesCorrectAverageTime_WhenWorkTimeAdded()
    {
        const string replica = "http://example.com/replica";
        var item = new ReplicaSendHistoryItem(replica);

        item.IncrementSuccessfulSend(TimeSpan.FromMilliseconds(100));
        item.IncrementSuccessfulSend(TimeSpan.FromMilliseconds(200));
        item.IncrementSuccessfulSend(TimeSpan.FromMilliseconds(300));
        var averageTime = item.CalculateAverageResponseTime();

        averageTime.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Test]
    public void CalculateAverageResponseTime_ShouldIsThreadSafe()
    {
        const string replica = "http://example.com/replica";
        var item = new ReplicaSendHistoryItem(replica);

        var threads = Enumerable.Range(0, 100)
            .Select(_ => new Thread(_ => item.IncrementSuccessfulSend(TimeSpan.FromMilliseconds(1))))
            .ToList();
        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());

        item.CalculateAverageResponseTime().Should().Be(TimeSpan.FromMilliseconds(1));
        item.SendWithWorkTimeCount.Should().Be(100);
    }

    [Test]
    public void IncrementSendCount_ShouldIsThreadSafe()
    {
        const string replica = "http://example.com/replica";
        var item = new ReplicaSendHistoryItem(replica);

        var threads = Enumerable.Range(0, 100).Select(_ => new Thread(item.IncrementSendCount)).ToList();
        threads.ForEach(x => x.Start());
        threads.ForEach(x => x.Join());

        item.SendCount.Should().Be(100);
    }
}