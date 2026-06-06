using RuriLib.Parallelization.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable xUnit1051 // Most tests intentionally exercise the default-token lifecycle overloads.

namespace RuriLib.Parallelization.Tests;

public class ParallelizerTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    private readonly Func<int, CancellationToken, Task<bool>> _parityCheck
        = (number, _) => Task.FromResult(number % 2 == 0);

    private const ParallelizerType _type = ParallelizerType.TaskBased;
    private int _progressCount;
    private bool _lastResult;
    private bool _completedFlag;
    private Exception? _lastException;

    private void OnProgress(object? sender, float value) => _progressCount++;

    private void OnResult(object? sender, ResultDetails<int, bool> value) => _lastResult = value.Result;

    private void OnCompleted(object? sender, EventArgs e) => _completedFlag = true;

    private void OnException(object? sender, Exception ex) => _lastException = ex;

    private static CancellationTokenSource CreateTestTimeout()
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        cts.CancelAfter(10000);
        return cts;
    }

    private static void UpdateMax(ref int target, int value)
    {
        var current = Volatile.Read(ref target);

        while (value > current)
        {
            var original = Interlocked.CompareExchange(ref target, value, current);

            if (original == current)
            {
                return;
            }

            current = original;
        }
    }

    [Theory]
    [InlineData(ParallelizerType.ThreadBased)]
    [InlineData(ParallelizerType.ParallelBased)]
    public async Task Run_QuickTasks_ThreadAndParallel_CompleteAndReportAllResults(ParallelizerType type)
    {
        const int count = 100;
        var results = new ConcurrentBag<ResultDetails<int, bool>>();
        var statuses = new ConcurrentBag<ParallelizerStatus>();
        var progressCount = 0;
        var completed = false;
        Exception? exception = null;

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: type,
            workItems: Enumerable.Range(1, count),
            workFunction: _parityCheck,
            degreeOfParallelism: 4,
            totalAmount: count,
            skip: 0);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.NewResult += (_, result) => results.Add(result);
        parallelizer.StatusChanged += (_, status) => statuses.Add(status);
        parallelizer.Completed += (_, _) => completed = true;
        parallelizer.Error += (_, ex) => exception = ex;

        await parallelizer.Start();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, progressCount);
        Assert.Equal(count, results.Count);
        Assert.All(results, result => Assert.Equal(result.Item % 2 == 0, result.Result));
        Assert.Contains(ParallelizerStatus.Running, statuses);
        Assert.Contains(ParallelizerStatus.Idle, statuses);
        Assert.True(completed);
        Assert.Null(exception);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
        Assert.Equal(1, parallelizer.Progress);
    }

    [Theory]
    [InlineData(ParallelizerType.ThreadBased)]
    [InlineData(ParallelizerType.ParallelBased)]
    public async Task Run_LongTasks_ThreadAndParallel_AbortStopsActiveWork(ParallelizerType type)
    {
        const int degreeOfParallelism = 4;
        var startedCount = 0;
        var progressCount = 0;
        var taskErrorCount = 0;
        var completed = false;
        var allWorkersStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> BlockingWork(int _, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref startedCount) == degreeOfParallelism)
            {
                allWorkersStarted.TrySetResult();
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: type,
            workItems: Enumerable.Range(1, 100),
            workFunction: BlockingWork,
            degreeOfParallelism: degreeOfParallelism,
            totalAmount: 100,
            skip: 0);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.TaskError += (_, _) => Interlocked.Increment(ref taskErrorCount);
        parallelizer.Completed += (_, _) => completed = true;

        await parallelizer.Start();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        cts.CancelAfter(10000);
        await allWorkersStarted.Task.WaitAsync(cts.Token);

        await parallelizer.Abort();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(degreeOfParallelism, startedCount);
        Assert.Equal(degreeOfParallelism, progressCount);
        Assert.Equal(degreeOfParallelism, taskErrorCount);
        Assert.True(completed);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task ParallelBased_UnsupportedOperations_Throw()
    {
        const int degreeOfParallelism = 4;
        var startedCount = 0;
        var allWorkersStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> BlockingWork(int _, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref startedCount) == degreeOfParallelism)
            {
                allWorkersStarted.TrySetResult();
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: ParallelizerType.ParallelBased,
            workItems: Enumerable.Range(1, 100),
            workFunction: BlockingWork,
            degreeOfParallelism: degreeOfParallelism,
            totalAmount: 100,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await allWorkersStarted.Task.WaitAsync(cts.Token);

        await Assert.ThrowsAsync<NotSupportedException>(() => parallelizer.Pause());
        await Assert.ThrowsAsync<NotSupportedException>(() => parallelizer.Stop());
        await Assert.ThrowsAsync<NotSupportedException>(() => parallelizer.ChangeDegreeOfParallelism(2));

        await parallelizer.Abort();
    }

    [Fact]
    public async Task Run_QuickTasks_CompleteAndCall()
    {
        const int count = 100;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: _parityCheck,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0);

        _progressCount = 0;
        _completedFlag = false;
        _lastException = null;
        parallelizer.ProgressChanged += OnProgress;
        parallelizer.NewResult += OnResult;
        parallelizer.Completed += OnCompleted;
        parallelizer.Error += OnException;

        await parallelizer.Start();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(100, _progressCount);
        Assert.True(_completedFlag);
        Assert.Null(_lastException);
        Assert.True(_lastResult);
    }

    [Fact]
    public async Task Run_MaxDegreeOfParallelismOne_RefillsQueueAndProcessesAllItems()
    {
        const int count = 100;
        var progressCount = 0;
        var results = new ConcurrentBag<ResultDetails<int, bool>>();
        var completed = false;
        Exception? exception = null;

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: _parityCheck,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0,
            maxDegreeOfParallelism: 1);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.NewResult += (_, result) => results.Add(result);
        parallelizer.Completed += (_, _) => completed = true;
        parallelizer.Error += (_, ex) => exception = ex;

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, progressCount);
        Assert.Equal(count, results.Count);
        Assert.True(completed);
        Assert.Null(exception);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_WorkItemsFewerThanTotalAmount_CompletesWhenEnumerationEnds()
    {
        var progressCount = 0;
        var completed = false;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 3),
            workFunction: _parityCheck,
            degreeOfParallelism: 2,
            totalAmount: 10,
            skip: 0);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.Completed += (_, _) => completed = true;

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(3, progressCount);
        Assert.Equal(0.3F, parallelizer.Progress);
        Assert.True(completed);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_CompletedEvent_IsRaisedAfterStatusBecomesIdle()
    {
        ParallelizerStatus? statusInCompletedHandler = null;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 1),
            workFunction: _parityCheck,
            degreeOfParallelism: 1,
            totalAmount: 1,
            skip: 0);

        parallelizer.Completed += (_, _) =>
        {
            statusInCompletedHandler = parallelizer.Status;
        };

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(ParallelizerStatus.Idle, statusInCompletedHandler);
    }

    [Fact]
    public async Task Run_QuickTasks_StopwatchStops()
    {
        const int count = 100;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: _parityCheck,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0);

        await parallelizer.Start();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);

        var elapsed = parallelizer.Elapsed;
        await Task.Delay(100, cts.Token);
        Assert.Equal(elapsed, parallelizer.Elapsed);
    }

    [Fact]
    public async Task Run_CompletedParallelizerAgain_ResetsProgressForNewRun()
    {
        const int count = 3;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: _parityCheck,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0);

        using var cts = CreateTestTimeout();

        await parallelizer.Start();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(1, parallelizer.Progress);

        await parallelizer.Start();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(1, parallelizer.Progress);
        Assert.Equal(count, parallelizer.CPM);
    }

    [Fact]
    public async Task Run_QuickTasks_UpdatesCpmForProcessedItems()
    {
        const int count = 10;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: _parityCheck,
            degreeOfParallelism: 4,
            totalAmount: count,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, parallelizer.CPM);
    }

    [Fact]
    public async Task Run_TaskErrors_UpdateCpmForProcessedItems()
    {
        const int count = 10;
        var taskErrorCount = 0;

        Task<bool> ThrowingWork(int _, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The work item failed");

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: ThrowingWork,
            degreeOfParallelism: 4,
            totalAmount: count,
            skip: 0);

        parallelizer.TaskError += (_, _) => Interlocked.Increment(ref taskErrorCount);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, taskErrorCount);
        Assert.Equal(count, parallelizer.CPM);
    }

    [Fact]
    public async Task Run_WorkItemsEnumerationError_ReportsErrorAndReturnsIdle()
    {
        var completed = false;
        Exception? exception = null;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: ThrowingWorkItems(),
            workFunction: _parityCheck,
            degreeOfParallelism: 1,
            totalAmount: 1,
            skip: 0);

        parallelizer.Completed += (_, _) => completed = true;
        parallelizer.Error += (_, ex) => exception = ex;

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.True(completed);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);

        static IEnumerable<int> ThrowingWorkItems()
        {
            throw new InvalidOperationException("The work items could not be enumerated");
#pragma warning disable CS0162 // Unreachable code detected
            yield return 1;
#pragma warning restore CS0162 // Unreachable code detected
        }
    }

    [Fact]
    public async Task Run_LongTasks_StopWaitsForCurrentWorkAndLeavesPendingItems()
    {
        const int degreeOfParallelism = 4;
        const int completedBeforeStop = 4;
        const int expectedProcessed = completedBeforeStop + degreeOfParallelism;
        var startedCount = 0;
        var progressCount = 0;
        var completed = false;
        Exception? exception = null;
        var currentWorkersStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorkers = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> BlockingWork(int item, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref startedCount) == expectedProcessed)
            {
                currentWorkersStarted.TrySetResult();
            }

            if (item <= completedBeforeStop)
            {
                return true;
            }

            await releaseWorkers.Task.WaitAsync(cancellationToken);
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 100),
            workFunction: BlockingWork,
            degreeOfParallelism: degreeOfParallelism,
            totalAmount: 100,
            skip: 0);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.Completed += (_, _) => completed = true;
        parallelizer.Error += (_, ex) => exception = ex;

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await currentWorkersStarted.Task.WaitAsync(cts.Token);

        var stopTask = parallelizer.Stop();
        releaseWorkers.SetResult();
        await stopTask.WaitAsync(cts.Token);

        Assert.Equal(expectedProcessed, startedCount);
        Assert.Equal(expectedProcessed, progressCount);
        Assert.Equal((float)expectedProcessed / 100, parallelizer.Progress);
        Assert.True(completed);
        Assert.Null(exception);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_LongTasks_AbortBeforeCompletion()
    {
        const int degreeOfParallelism = 4;
        var startedCount = 0;
        var progressCount = 0;
        var taskErrorCount = 0;
        var completed = false;
        var allWorkersStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> BlockingWork(int _, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref startedCount) == degreeOfParallelism)
            {
                allWorkersStarted.TrySetResult();
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 100),
            workFunction: BlockingWork,
            degreeOfParallelism: degreeOfParallelism,
            totalAmount: 100,
            skip: 0);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.TaskError += (_, _) => Interlocked.Increment(ref taskErrorCount);
        parallelizer.Completed += (_, _) => completed = true;

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await allWorkersStarted.Task.WaitAsync(cts.Token);

        await parallelizer.Abort();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(degreeOfParallelism, startedCount);
        Assert.Equal(degreeOfParallelism, progressCount);
        Assert.Equal(degreeOfParallelism, taskErrorCount);
        Assert.True(completed);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_NonCooperativeWorkFinishesAfterAbort_StaysIdle()
    {
        var workerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var progressCount = 0;
        var completed = false;

        async Task<bool> NonCooperativeWork(int _, CancellationToken cancellationToken)
        {
            workerStarted.TrySetResult();
            await releaseWorker.Task;
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 1),
            workFunction: NonCooperativeWork,
            degreeOfParallelism: 1,
            totalAmount: 1,
            skip: 0);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.Completed += (_, _) => completed = true;

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await workerStarted.Task.WaitAsync(cts.Token);

        await parallelizer.Abort();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.True(completed);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
        Assert.Equal(0, Volatile.Read(ref progressCount));

        // This worker finishes after hard abort and after the parallelizer has gone idle.
        // It must not move the parallelizer out of its completed state or touch disposed resources.
        releaseWorker.SetResult();

        while (Volatile.Read(ref progressCount) == 0)
        {
            await Task.Delay(10, cts.Token);
        }

        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_PauseInProgress_AbortUnblocksPause()
    {
        var workerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> NonCooperativeWork(int _, CancellationToken cancellationToken)
        {
            workerStarted.TrySetResult();
            await releaseWorker.Task;
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 1),
            workFunction: NonCooperativeWork,
            degreeOfParallelism: 1,
            totalAmount: 1,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await workerStarted.Task.WaitAsync(cts.Token);

        var pauseTask = parallelizer.Pause();
        await Task.Delay(100, cts.Token);

        await parallelizer.Abort();
        await pauseTask.WaitAsync(cts.Token);

        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);

        releaseWorker.SetResult();
    }

    [Fact]
    public async Task Run_PauseCancellation_CancelsCallerWaitButPauseContinues()
    {
        var firstWorkerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondWorkerStarted = false;
        var releaseFirstWorker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> Work(int item, CancellationToken cancellationToken)
        {
            if (item == 1)
            {
                firstWorkerStarted.TrySetResult();
                await releaseFirstWorker.Task;
            }
            else
            {
                secondWorkerStarted = true;
            }

            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 2),
            workFunction: Work,
            degreeOfParallelism: 1,
            totalAmount: 2,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await firstWorkerStarted.Task.WaitAsync(cts.Token);

        using var pauseCts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        pauseCts.CancelAfter(100);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => parallelizer.Pause(pauseCts.Token));

        Assert.Equal(ParallelizerStatus.Pausing, parallelizer.Status);
        releaseFirstWorker.SetResult();

        while (parallelizer.Status != ParallelizerStatus.Paused)
        {
            await Task.Delay(10, cts.Token);
        }

        Assert.False(secondWorkerStarted);

        await parallelizer.Abort();
    }

    [Fact]
    public async Task Run_StopCancellation_CancelsCallerWaitButStopContinues()
    {
        var workerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> Work(int _, CancellationToken cancellationToken)
        {
            workerStarted.TrySetResult();
            await releaseWorker.Task;
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 1),
            workFunction: Work,
            degreeOfParallelism: 1,
            totalAmount: 1,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await workerStarted.Task.WaitAsync(cts.Token);

        using var stopCts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        stopCts.CancelAfter(100);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => parallelizer.Stop(stopCts.Token));

        Assert.Equal(ParallelizerStatus.Stopping, parallelizer.Status);
        releaseWorker.SetResult();

        await parallelizer.WaitCompletion(cts.Token);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_IncreaseConcurrentThreads_AllowsMoreConcurrentWork()
    {
        const int count = 4;
        var startedCount = 0;
        var firstWorkerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var allWorkersStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorkers = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> BlockingWork(int _, CancellationToken cancellationToken)
        {
            var currentStarted = Interlocked.Increment(ref startedCount);

            if (currentStarted == 1)
            {
                firstWorkerStarted.TrySetResult();
            }

            if (currentStarted == count)
            {
                allWorkersStarted.TrySetResult();
            }

            await releaseWorkers.Task.WaitAsync(cancellationToken);
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: BlockingWork,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await firstWorkerStarted.Task.WaitAsync(cts.Token);

        Assert.Equal(1, Volatile.Read(ref startedCount));
        await parallelizer.ChangeDegreeOfParallelism(4);
        await allWorkersStarted.Task.WaitAsync(cts.Token);

        releaseWorkers.SetResult();
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, startedCount);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_CpmLimited_DelaysBeforeStartingMoreWorkUntilStopped()
    {
        var startedCount = 0;
        var progressCount = 0;
        var firstWorkerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<bool> Work(int _, CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref startedCount) == 1)
            {
                firstWorkerStarted.TrySetResult();
            }

            return Task.FromResult(true);
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 100),
            workFunction: Work,
            degreeOfParallelism: 1,
            totalAmount: 100,
            skip: 0);

        parallelizer.CPMLimit = 1;
        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await firstWorkerStarted.Task.WaitAsync(cts.Token);

        await Task.Delay(100, cts.Token);
        Assert.Equal(1, Volatile.Read(ref startedCount));
        Assert.Equal(1, parallelizer.CPM);

        await parallelizer.Stop();

        Assert.Equal(1, Volatile.Read(ref startedCount));
        Assert.Equal(1, Volatile.Read(ref progressCount));
        Assert.Equal(1, parallelizer.CPM);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_DecreaseConcurrentThreads_LimitsNewConcurrentWork()
    {
        const int count = 6;
        const int originalDegreeOfParallelism = 3;
        var startedCount = 0;
        var runningCount = 0;
        var maxRunningAfterDecrease = 0;
        var trackAfterDecrease = 0;
        var firstBatchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fourthWorkerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorkers = Enumerable.Range(1, count)
            .ToDictionary(item => item, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
        var startedItems = new ConcurrentQueue<int>();

        async Task<bool> BlockingWork(int item, CancellationToken cancellationToken)
        {
            startedItems.Enqueue(item);
            var currentStarted = Interlocked.Increment(ref startedCount);
            var running = Interlocked.Increment(ref runningCount);

            if (Volatile.Read(ref trackAfterDecrease) == 1)
            {
                UpdateMax(ref maxRunningAfterDecrease, running);
            }

            if (currentStarted == originalDegreeOfParallelism)
            {
                firstBatchStarted.TrySetResult();
            }

            if (currentStarted == originalDegreeOfParallelism + 1)
            {
                fourthWorkerStarted.TrySetResult();
            }

            try
            {
                await releaseWorkers[item].Task.WaitAsync(cancellationToken);
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref runningCount);
            }
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: BlockingWork,
            degreeOfParallelism: originalDegreeOfParallelism,
            totalAmount: count,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await firstBatchStarted.Task.WaitAsync(cts.Token);

        var decreaseTask = parallelizer.ChangeDegreeOfParallelism(1);

        Assert.True(startedItems.TryDequeue(out var firstItem));
        Assert.True(startedItems.TryDequeue(out var secondItem));
        releaseWorkers[firstItem].SetResult();
        releaseWorkers[secondItem].SetResult();
        await decreaseTask.WaitAsync(cts.Token);

        Volatile.Write(ref trackAfterDecrease, 1);

        Assert.True(startedItems.TryDequeue(out var thirdItem));
        releaseWorkers[thirdItem].SetResult();
        await fourthWorkerStarted.Task.WaitAsync(cts.Token);

        foreach (var releaseWorker in releaseWorkers.Values)
        {
            releaseWorker.TrySetResult();
        }

        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, startedCount);
        Assert.Equal(1, maxRunningAfterDecrease);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_PauseAndResume_CompleteAll()
    {
        const int count = 3;
        var progressCount = 0;
        var startedCount = 0;
        var completed = false;
        Exception? exception = null;
        var firstWorkerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondWorkerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorkers = Enumerable.Range(1, count)
            .ToDictionary(item => item, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        async Task<bool> BlockingWork(int item, CancellationToken cancellationToken)
        {
            var currentStarted = Interlocked.Increment(ref startedCount);

            if (currentStarted == 1)
            {
                firstWorkerStarted.TrySetResult();
            }

            if (currentStarted == 2)
            {
                secondWorkerStarted.TrySetResult();
            }

            await releaseWorkers[item].Task.WaitAsync(cancellationToken);
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: BlockingWork,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0);

        parallelizer.ProgressChanged += (_, _) => Interlocked.Increment(ref progressCount);
        parallelizer.Completed += (_, _) => completed = true;
        parallelizer.Error += (_, ex) => exception = ex;

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await firstWorkerStarted.Task.WaitAsync(cts.Token);

        var pauseTask = parallelizer.Pause();
        releaseWorkers[1].SetResult();
        await pauseTask.WaitAsync(cts.Token);

        Assert.Equal(ParallelizerStatus.Paused, parallelizer.Status);
        Assert.Equal(1, startedCount);
        Assert.Equal(1, progressCount);

        await parallelizer.Resume();
        await secondWorkerStarted.Task.WaitAsync(cts.Token);

        foreach (var releaseWorker in releaseWorkers.Values)
        {
            releaseWorker.TrySetResult();
        }

        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, progressCount);
        Assert.True(completed);
        Assert.Null(exception);
        Assert.Equal(ParallelizerStatus.Idle, parallelizer.Status);
    }

    [Fact]
    public async Task Run_Pause_StopwatchStops()
    {
        const int count = 3;
        var firstWorkerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseWorkers = Enumerable.Range(1, count)
            .ToDictionary(item => item, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        async Task<bool> BlockingWork(int item, CancellationToken cancellationToken)
        {
            if (item == 1)
            {
                firstWorkerStarted.TrySetResult();
            }

            await releaseWorkers[item].Task.WaitAsync(cancellationToken);
            return true;
        }

        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: BlockingWork,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0);

        await parallelizer.Start();

        using var cts = CreateTestTimeout();
        await firstWorkerStarted.Task.WaitAsync(cts.Token);

        var pauseTask = parallelizer.Pause();
        releaseWorkers[1].SetResult();
        await pauseTask.WaitAsync(cts.Token);

        var elapsed = parallelizer.Elapsed;
        await Task.Delay(100, cts.Token);
        Assert.Equal(elapsed, parallelizer.Elapsed);

        await parallelizer.Abort();
    }
}
