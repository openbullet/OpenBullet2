using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization;

/// <summary>
/// Parallelizer that expoits batches of multiple tasks and the WaitAll function.
/// </summary>
public class TaskBasedParallelizer<TInput, TOutput> : Parallelizer<TInput, TOutput>
{
    #region Private Fields
    private int BatchSize => MaxDegreeOfParallelism * 2;
    private SemaphoreSlim? _semaphore;
    private readonly ConcurrentQueue<TInput> _queue = new();
    private int _savedDop;
    private bool _dopDecreaseRequested;
    #endregion

    #region Constructors
    /// <inheritdoc/>
    public TaskBasedParallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
        int degreeOfParallelism, long totalAmount, int skip = 0, int maxDegreeOfParallelism = 200)
        : base(workItems, workFunction, degreeOfParallelism, totalAmount, skip, maxDegreeOfParallelism)
    {

    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override async Task Start()
    {
        await base.Start().ConfigureAwait(false);

        Stopwatch.Restart();
        Status = ParallelizerStatus.Running;
        _ = Task.Run(Run).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task Pause()
    {
        await base.Pause().ConfigureAwait(false);

        Status = ParallelizerStatus.Pausing;
        _savedDop = DegreeOfParallelism;
        await ChangeDegreeOfParallelism(0).ConfigureAwait(false);
        Status = ParallelizerStatus.Paused;
        Stopwatch.Stop();
    }

    /// <inheritdoc/>
    public override async Task Resume()
    {
        await base.Resume().ConfigureAwait(false);

        Status = ParallelizerStatus.Resuming;
        await ChangeDegreeOfParallelism(_savedDop).ConfigureAwait(false);
        Status = ParallelizerStatus.Running;
        Stopwatch.Start();
    }

    /// <inheritdoc/>
    public override async Task Stop()
    {
        await base.Stop().ConfigureAwait(false);

        Status = ParallelizerStatus.Stopping;
        await SoftCts.CancelAsync();
        await WaitCompletion().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task Abort()
    {
        await base.Abort().ConfigureAwait(false);

        Status = ParallelizerStatus.Stopping;
        await HardCts.CancelAsync();
        await SoftCts.CancelAsync();
        await WaitCompletion().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task ChangeDegreeOfParallelism(int newValue)
    {
        await base.ChangeDegreeOfParallelism(newValue);

        switch (Status)
        {
            case ParallelizerStatus.Idle:
                DegreeOfParallelism = newValue;
                return;
            
            case ParallelizerStatus.Paused:
                _savedDop = newValue;
                return;
        }

        if (newValue == DegreeOfParallelism)
        {
            return;
        }

        if (_semaphore is null)
        {
            DegreeOfParallelism = newValue;
            return;
        }
        
        if (newValue > DegreeOfParallelism)
        {
            _semaphore.Release(newValue - DegreeOfParallelism);
        }
        else
        {
            _dopDecreaseRequested = true;
            for (var i = 0; i < DegreeOfParallelism - newValue; ++i)
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
            }
            _dopDecreaseRequested = false;
        }

        DegreeOfParallelism = newValue;
    }
    #endregion

    #region Private Methods
    // Run is executed in fire and forget mode (not awaited)
    private async void Run()
    {
        _dopDecreaseRequested = false;

        // Skip the items
        using var items = WorkItems.Skip(Skip).GetEnumerator();

        // Clear the queue
        _queue.Clear();

        // Enqueue the first batch (at most BatchSize items)
        while (_queue.Count < BatchSize && items.MoveNext())
        {
            _queue.Enqueue(items.Current);
        }

        _semaphore = new SemaphoreSlim(DegreeOfParallelism, MaxDegreeOfParallelism);
        
        try
        {   
            // While there are items in the queue, and we didn't cancel, dequeue one, wait and then
            // queue another task if there are more to queue
            while (!_queue.IsEmpty && !SoftCts.IsCancellationRequested)
            {
                WAIT:

                // Wait for the semaphore
                await _semaphore!.WaitAsync(SoftCts.Token).ConfigureAwait(false);

                if (SoftCts.IsCancellationRequested)
                    break;

                if (_dopDecreaseRequested || IsCpmLimited())
                {
                    UpdateCpm();
                    _semaphore!.Release();
                    goto WAIT;
                }

                // If the current batch is running out
                if (_queue.Count < MaxDegreeOfParallelism)
                {
                    // Queue more items until the BatchSize is reached OR until the enumeration finished
                    while (_queue.Count < BatchSize && items.MoveNext())
                    {
                        _queue.Enqueue(items.Current);
                    }
                }

                // If we can dequeue an item, run it
                if (_queue.TryDequeue(out var item))
                {
                    // The task will release its slot no matter what
                    _ = TaskFunction.Invoke(item)
                        // ReSharper disable once AccessToDisposedClosure
                        // (the semaphore is only disposed after the loop finishes)
                        .ContinueWith(_ => _semaphore?.Release())
                        .ConfigureAwait(false);
                }
                else
                {
                    _semaphore?.Release();
                }
            }

            // Wait for every remaining task from the last batch to finish unless aborted
            while (Progress < 1 && !HardCts.IsCancellationRequested)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Wait for current tasks to finish unless aborted
            while (_semaphore!.CurrentCount < DegreeOfParallelism && !HardCts.IsCancellationRequested)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
        finally
        {
            OnCompleted();
            Status = ParallelizerStatus.Idle;
            HardCts.Dispose();
            SoftCts.Dispose();
            _semaphore?.Dispose();
            _semaphore = null;
            Stopwatch.Stop();
        }
    }
    #endregion
}
