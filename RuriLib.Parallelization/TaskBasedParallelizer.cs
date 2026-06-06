using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization;

/// <summary>
/// Parallelizer that schedules asynchronous tasks up to a dynamic degree of parallelism.
/// </summary>
public class TaskBasedParallelizer<TInput, TOutput> : Parallelizer<TInput, TOutput>
{
    private int BatchSize => MaxDegreeOfParallelism * 2;
    private readonly Queue<TInput> _queue = new();
    private int _savedDop;
    private int _activeTaskCount;

    // The scheduler signal is a reusable async pulse. It wakes the run loop, DOP decrease,
    // pause, and completion waiters whenever active work count or desired DOP may have changed.
    private readonly Lock _schedulerSignalLock = new();
    private TaskCompletionSource _schedulerSignal = CreateSchedulerSignal();

    /// <inheritdoc/>
    public TaskBasedParallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
        int degreeOfParallelism, long totalAmount, int skip = 0, int maxDegreeOfParallelism = 200)
        : base(workItems, workFunction, degreeOfParallelism, totalAmount, skip, maxDegreeOfParallelism)
    {

    }

    /// <inheritdoc/>
    public override async Task Start(CancellationToken cancellationToken = default)
    {
        await base.Start(cancellationToken).ConfigureAwait(false);

        Stopwatch.Restart();
        SetStatus(ParallelizerStatus.Running);
        _ = Task.Run(Run);
    }

    /// <inheritdoc/>
    public override async Task Pause(CancellationToken cancellationToken = default)
    {
        await base.Pause(cancellationToken).ConfigureAwait(false);

        if (!TrySetStatusUnlessIdle(ParallelizerStatus.Pausing))
        {
            return;
        }

        _savedDop = DegreeOfParallelism;

        try
        {
            await FinishPause(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _ = FinishPause(CancellationToken.None);
            throw;
        }
    }

    private async Task FinishPause(CancellationToken cancellationToken)
    {
        await ChangeDegreeOfParallelism(0, cancellationToken).ConfigureAwait(false);
        await WaitActiveCountAtMost(0, cancellationToken).ConfigureAwait(false);

        if (Status == ParallelizerStatus.Idle)
        {
            SetDegreeOfParallelism(_savedDop);
            return;
        }

        if (!TrySetStatusIf(ParallelizerStatus.Paused, status => status == ParallelizerStatus.Pausing))
        {
            if (Status == ParallelizerStatus.Idle)
            {
                SetDegreeOfParallelism(_savedDop);
            }

            return;
        }

        Stopwatch.Stop();
    }

    /// <inheritdoc/>
    public override async Task Resume(CancellationToken cancellationToken = default)
    {
        await base.Resume(cancellationToken).ConfigureAwait(false);

        SetStatus(ParallelizerStatus.Resuming);
        await ChangeDegreeOfParallelism(_savedDop, cancellationToken).ConfigureAwait(false);
        SetStatus(ParallelizerStatus.Running);
        Stopwatch.Start();
    }

    /// <inheritdoc/>
    public override async Task Stop(CancellationToken cancellationToken = default)
    {
        await base.Stop(cancellationToken).ConfigureAwait(false);

        if (!TrySetStatusUnlessIdle(ParallelizerStatus.Stopping))
        {
            return;
        }

        await CancelIfNotDisposed(SoftCts).ConfigureAwait(false);
        await WaitCompletion(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task Abort(CancellationToken cancellationToken = default)
    {
        await base.Abort(cancellationToken).ConfigureAwait(false);

        if (!TrySetStatusUnlessIdle(ParallelizerStatus.Stopping))
        {
            return;
        }

        var softCancelTask = CancelIfNotDisposed(SoftCts);
        var hardCancelTask = CancelIfNotDisposed(HardCts);

        // Request both cancellations before waiting. HardCts wakes cooperative workers, while
        // SoftCts stops the scheduler from starting replacements as those workers finish.
        await hardCancelTask.ConfigureAwait(false);
        await softCancelTask.ConfigureAwait(false);
        SignalScheduler();
        await WaitActiveCountAtMost(0, TaskBasedParallelizerDefaults.HardAbortCompletionGracePeriod, cancellationToken).ConfigureAwait(false);
        await WaitCompletion(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task ChangeDegreeOfParallelism(int newValue, CancellationToken cancellationToken = default)
    {
        await base.ChangeDegreeOfParallelism(newValue, cancellationToken);

        switch (Status)
        {
            case ParallelizerStatus.Idle:
                DegreeOfParallelism = newValue;
                return;

            case ParallelizerStatus.Paused:
                _savedDop = newValue;
                return;
        }

        var previousValue = GetDegreeOfParallelism();

        if (newValue == previousValue)
        {
            return;
        }

        SetDegreeOfParallelism(newValue);
        SignalScheduler();

        if (newValue < previousValue)
        {
            // Decrease and pause are complete only after already-running work falls to the new limit.
            // New work is naturally held back because the scheduler reads DegreeOfParallelism directly.
            await WaitActiveCountAtMost(newValue, cancellationToken).ConfigureAwait(false);
        }
    }

    // Run is executed in fire and forget mode (not awaited)
    private async Task Run()
    {
        try
        {
            ResetSchedulerSignal();
            _activeTaskCount = 0;

            // Skip the items
            using var items = WorkItems.Skip(Skip).GetEnumerator();

            // Clear the queue
            _queue.Clear();

            // Enqueue the first batch (at most BatchSize items)
            while (_queue.Count < BatchSize && items.MoveNext())
            {
                _queue.Enqueue(items.Current);
            }

            // While there are items in the queue, and we didn't cancel, dequeue one, wait until
            // there is available DOP, and then queue another task if there are more to queue
            while (_queue.Count > 0 && !SoftCts.IsCancellationRequested)
            {
                await WaitForAvailableSlot(SoftCts.Token).ConfigureAwait(false);

                var cpmThrottleDelay = GetCpmThrottleDelay();

                if (cpmThrottleDelay > TimeSpan.Zero)
                {
                    await Task.Delay(cpmThrottleDelay, SoftCts.Token).ConfigureAwait(false);
                    continue;
                }

                // If the current batch is running out
                if (_queue.Count <= MaxDegreeOfParallelism)
                {
                    // Queue more items until the BatchSize is reached OR until the enumeration finished
                    while (_queue.Count < BatchSize && items.MoveNext())
                    {
                        _queue.Enqueue(items.Current);
                    }
                }

                var item = _queue.Dequeue();
                Interlocked.Increment(ref _activeTaskCount);

                _ = RunItem(item);
            }

            await WaitCurrentWorkCompletion().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Wait for current tasks to finish unless aborted
            await WaitCurrentWorkCompletion().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
        finally
        {
            HardCts.Dispose();
            SoftCts.Dispose();
            Stopwatch.Stop();
            SetStatus(ParallelizerStatus.Idle);

            OnCompleted();
        }
    }

    private async Task WaitCurrentWorkCompletion()
    {
        while (Volatile.Read(ref _activeTaskCount) > 0 && !HardCts.IsCancellationRequested)
        {
            // Take the current signal before checking again so a finishing task cannot be missed
            // between the condition check and the await.
            var waitTask = ResetAndGetSchedulerSignalTask();

            if (Volatile.Read(ref _activeTaskCount) == 0 || HardCts.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await waitTask.WaitAsync(HardCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (HardCts.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task RunItem(TInput item)
    {
        try
        {
            await TaskFunction.Invoke(item).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _activeTaskCount);
            SignalScheduler();
        }
    }

    private async Task WaitForAvailableSlot(CancellationToken cancellationToken)
    {
        while (GetDegreeOfParallelism() <= Volatile.Read(ref _activeTaskCount))
        {
            UpdateCpm();
            // Wait until either DOP increases or a running item completes and decrements the active count.
            var waitTask = ResetAndGetSchedulerSignalTask();

            if (GetDegreeOfParallelism() > Volatile.Read(ref _activeTaskCount))
            {
                return;
            }

            await waitTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task WaitActiveCountAtMost(int value, CancellationToken cancellationToken)
    {
        while (Volatile.Read(ref _activeTaskCount) > value && Status != ParallelizerStatus.Idle)
        {
            // Used by DOP decrease and pause. Hard abort is allowed to cut this wait short.
            var waitTask = ResetAndGetSchedulerSignalTask();

            if (Volatile.Read(ref _activeTaskCount) <= value ||
                Status == ParallelizerStatus.Idle ||
                HardCts.IsCancellationRequested)
            {
                return;
            }

            await waitTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task WaitActiveCountAtMost(int value, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var waitUntil = Environment.TickCount64 + (long)timeout.TotalMilliseconds;

        while (Volatile.Read(ref _activeTaskCount) > value)
        {
            var remaining = waitUntil - Environment.TickCount64;

            if (remaining <= 0)
            {
                return;
            }

            // Used only by hard abort: give cooperative tasks a brief chance to observe HardCts
            // and report progress, but do not wait indefinitely for non-cooperative tasks.
            var waitTask = ResetAndGetSchedulerSignalTask();

            if (Volatile.Read(ref _activeTaskCount) <= value)
            {
                return;
            }

            try
            {
                await waitTask.WaitAsync(TimeSpan.FromMilliseconds(remaining), cancellationToken).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                return;
            }
        }
    }

    private int GetDegreeOfParallelism()
    {
        lock (StatusLock)
        {
            return DegreeOfParallelism;
        }
    }

    private void SetDegreeOfParallelism(int value)
    {
        lock (StatusLock)
        {
            DegreeOfParallelism = value;
        }
    }

    private void SignalScheduler()
    {
        lock (_schedulerSignalLock)
        {
            _schedulerSignal.TrySetResult();
        }
    }

    private void ResetSchedulerSignal()
    {
        lock (_schedulerSignalLock)
        {
            _schedulerSignal = CreateSchedulerSignal();
        }
    }

    private Task ResetAndGetSchedulerSignalTask()
    {
        lock (_schedulerSignalLock)
        {
            // If the previous pulse was already consumed, create the next one before the caller awaits.
            if (_schedulerSignal.Task.IsCompleted)
            {
                _schedulerSignal = CreateSchedulerSignal();
            }

            return _schedulerSignal.Task;
        }
    }

    private static TaskCompletionSource CreateSchedulerSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task CancelIfNotDisposed(CancellationTokenSource cts)
    {
        try
        {
            await cts.CancelAsync().ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            // The run loop owns cleanup and can dispose the CTS while Stop/Abort is unwinding.
        }
    }

    private void SetStatus(ParallelizerStatus status)
    {
        SetStatusValue(status);
    }

    private bool TrySetStatusUnlessIdle(ParallelizerStatus status)
    {
        return TrySetStatusIf(status, current => current != ParallelizerStatus.Idle);
    }

    private bool TrySetStatusIf(ParallelizerStatus status, Func<ParallelizerStatus, bool> predicate) =>
        TrySetStatusValue(status, predicate);
}

internal static class TaskBasedParallelizerDefaults
{
    // Abort should not wait forever for work that ignores HardCts. This small window preserves
    // existing behavior for cooperative work while keeping hard abort bounded for non-cooperative work.
    internal static readonly TimeSpan HardAbortCompletionGracePeriod = TimeSpan.FromMilliseconds(250);
}
