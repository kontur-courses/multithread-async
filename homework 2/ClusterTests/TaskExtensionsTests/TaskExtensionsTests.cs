using ClusterClient.Clients.Extensions;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        public async Task WaitForFirstSuccessAsync_ShouldReturnFirstSuccessfulTaskResult_WithAllWorkingTasks()
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
        public async Task WaitForFirstSuccessAsync_ShouldReturnFirstSuccessfulTaskResult_WithOnlyOneWorkingTask()
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
        public void WaitForFirstSuccessAsync_ShouldThrowTimeoutException_WithAllBadTasks()
        {
            var tasks = new List<Task<string>>
            {
                Task.FromException<string>(new Exception("Task 1 failed")),
                Task.FromException<string>(new Exception("Task 2 failed")),
                Task.FromException<string>(new Exception("Task 3 failed")),
            };

            var exception = Assert.ThrowsAsync<TimeoutException>(
                () => tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation));
            exception.Message.Should().Be(defaultErrorMessage);
        }

        [Test]
        public void WaitForFirstSuccessAsync_ShouldThrowTimeoutException_WithAllSlowTasks()
        {
            var timeout = 500;
            cancellation.CancelAfter(timeout);
            var tasks = new List<Task<string>>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(2 * timeout, cancellation.Token);
                    return "Task 1 completed";
                }, cancellation.Token),
                Task.Run(async () =>
                {
                    await Task.Delay(3 * timeout, cancellation.Token);
                    return "Task 2 completed";
                }, cancellation.Token),
                Task.Run(async () =>
                {
                    await Task.Delay(4 * timeout, cancellation.Token);
                    return "Task 3 completed";
                }, cancellation.Token)
            };

            var exception = Assert.ThrowsAsync<TimeoutException>(
                () => tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation));
            cancellation.IsCancellationRequested.Should().BeTrue();
            exception.Message.Should().Be(defaultErrorMessage);
        }

        [Test]
        public void WaitForFirstSuccessAsync_ShouldThrowInvalidOperationException_WithEmptyList()
        {
            var tasks = new List<Task<string>>();
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => tasks.WaitForFirstSuccessAsync(defaultErrorMessage, cancellation));
            exception.Message.Should().Be("The task list cannot be empty");
        }
    }
}
