using ClusterClient.Clients.Extensions;
using ClusterClient.ReplicasPriorityManagers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterTests.TaskExtensionsTests
{
    [TestFixture]
    internal class TaskExtensionsTests
    {
        private CancellationTokenSource cancellation;
        private readonly string defaultErrorMessage = "All tasks failed";

        [SetUp]
        protected void SetUp()
        {
            cancellation = new CancellationTokenSource();
        }

        [Test]
        public async Task WaitForFirstSuccessAsync_ForList_ShouldReturnFirstSuccessfulTaskResult_WithAllWorkingTasks()
        {
            var runtime = 100;
            var tasks = new List<Task<string>>
            {
                Task.Delay(runtime,cancellation.Token).ContinueWith(_ => "Task 1 completed"),
                Task.Delay(2 * runtime,cancellation.Token).ContinueWith(_ => "Task 2 completed"),
                Task.Delay(3 * runtime,cancellation.Token).ContinueWith(_ => "Task 3 completed")
            };

            var result = await tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation);

            result.Should().Be("Task 1 completed");
            cancellation.IsCancellationRequested.Should().BeTrue();
        }

        [Test]
        public async Task WaitForFirstSuccessAsync_ForDict_ShouldReturnFirstSuccessfulTaskResult_WithAllWorkingTasks()
        {
            var runtime = 100;
            var tasks = new ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)>();

            tasks
                .TryAdd(
                    Task.Delay(2 * runtime, cancellation.Token).ContinueWith(_ => "Task 2 completed"),
                    ("Replica2", Stopwatch.StartNew()));
            tasks
                .TryAdd(
                    Task.Delay(runtime, cancellation.Token).ContinueWith(_ => "Task 1 completed"),
                    ("Replica1", Stopwatch.StartNew()));
            tasks
                .TryAdd(
                    Task.Delay(3 * runtime, cancellation.Token).ContinueWith(_ => "Task 3 completed"),
                    ("Replica3", Stopwatch.StartNew()));

            var result = await tasks.WaitForFirstSuccessAsync(
                defaultErrorMessage,
                cancellation,
                new ResponseTimeManagerByAverage());

            result.Should().Be("Task 1 completed");
            cancellation.IsCancellationRequested.Should().BeTrue();
        }

        [Test]
        public async Task WaitForFirstSuccessAsync_ForList_ShouldReturnFirstSuccessfulTaskResult_WithOnlyOneWorkingTask()
        {
            var tasks = new List<Task<string>>
            {
                Task.FromException<string>(new Exception("Task 1 failed")),
                Task.FromException<string>(new Exception("Task 2 failed")),
                Task.Delay(100, cancellation.Token).ContinueWith(_ => "Task 3 completed")
            };

            var result = await tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation);

            result.Should().Be("Task 3 completed");
        }

        [Test]
        public async Task WaitForFirstSuccessAsync_ForDict_ShouldReturnFirstSuccessfulTaskResult_WithOnlyOneWorkingTask()
        {
            var tasks = new ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)>();

            tasks
                .TryAdd(
                   Task.FromException<string>(new Exception("Task 1 failed")),
                    ("Replica1", Stopwatch.StartNew()));
            tasks
                .TryAdd(
                    Task.FromException<string>(new Exception("Task 2 failed")),
                    ("Replica2", Stopwatch.StartNew()));
            tasks
                .TryAdd(
                    Task.Delay(100, cancellation.Token).ContinueWith(_ => "Task 3 completed"),
                    ("Replica3", Stopwatch.StartNew()));

            var result = await tasks.WaitForFirstSuccessAsync(
                 defaultErrorMessage,
                 cancellation,
                 new ResponseTimeManagerByAverage());

            result.Should().Be("Task 3 completed");
        }

        [Test]
        public void WaitForFirstSuccessAsync_ForList_ShouldThrowTimeoutException_WithAllBadTasks()
        {
            var tasks = new List<Task<string>>();
            for (var i = 1; i < 4; i++)
            {
                tasks.Add(Task.FromException<string>(new Exception($"Task {i - 1} failed")));
            }

            var exception = Assert.ThrowsAsync<TimeoutException>(
                () => tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation));
            exception.Message.Should().Be(defaultErrorMessage);
        }

        [Test]
        public void WaitForFirstSuccessAsync_ForDict_ShouldThrowTimeoutException_WithAllBadTasks()
        {
            var tasks = new ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)>();

            for (var i = 1; i < 4; i++)
            {
                tasks
                    .TryAdd(
                        Task.FromException<string>(new Exception($"Task {i} failed")),
                        ($"Replica{i}", Stopwatch.StartNew()));
            }

            var exception = Assert.ThrowsAsync<TimeoutException>(
                () => tasks.WaitForFirstSuccessAsync(
                        defaultErrorMessage,
                        cancellation,
                        new ResponseTimeManagerByAverage()));
            exception.Message.Should().Be(defaultErrorMessage);
        }

        [Test]
        public void WaitForFirstSuccessAsync_ForList_ShouldThrowTimeoutException_WithAllSlowTasks()
        {
            var timeout = 500;
            cancellation.CancelAfter(timeout);
            var tasks = new List<Task<string>>();
            for (var i = 2; i < 5; i++)
            {
                var count = i;
                tasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(count * timeout, cancellation.Token);
                    return $"Task {count - 1} completed";
                }, cancellation.Token));
            }

            var exception = Assert.ThrowsAsync<TimeoutException>(
                () => tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation));
            cancellation.IsCancellationRequested.Should().BeTrue();
            exception.Message.Should().Be(defaultErrorMessage);
        }

        [Test]
        public void WaitForFirstSuccessAsync_ForDict_ShouldThrowTimeoutException_WithAllSlowTasks()
        {
            var timeout = 500;
            cancellation.CancelAfter(timeout);

            var tasks = new ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)>();

            for (var i = 2; i < 5; i++)
            {
                var count = i;
                tasks
                    .TryAdd(
                        Task.Run(async () =>
                        {
                            await Task.Delay(count * timeout, cancellation.Token);
                            return $"Task {count - 1} completed";
                        }, cancellation.Token),
                        ($"Replica{count - 1}", Stopwatch.StartNew()));
            }

            var exception = Assert.ThrowsAsync<TimeoutException>(
                () => tasks.WaitForFirstSuccessAsync(
                         defaultErrorMessage,
                         cancellation,
                         new ResponseTimeManagerByAverage()));
            cancellation.IsCancellationRequested.Should().BeTrue();
            exception.Message.Should().Be(defaultErrorMessage);
        }

        [Test]
        public void WaitForFirstSuccessAsync_ForList_ShouldThrowInvalidOperationException_WithEmptyList()
        {
            var tasks = new List<Task<string>>();
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation));
            exception.Message.Should().Be("The task list cannot be empty");
        }

        [Test]
        public void WaitForFirstSuccessAsync_ForDict_ShouldThrowInvalidOperationException_WithEmptyList()
        {
            var tasks = new ConcurrentDictionary<Task<string>, (string address, Stopwatch stopwatch)>();
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => tasks.WaitForFirstSuccessAsync(
                         defaultErrorMessage,
                         cancellation,
                         new ResponseTimeManagerByAverage()));
            exception.Message.Should().Be("The task dictionary cannot be empty");
        }
    }
}
