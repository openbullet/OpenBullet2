using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization;

/// <summary>
/// Parallelizer that expoits a custom pool of threads.
/// </summary>
public class ThreadBasedParallelizer<TInput, TOutput> : Parallelizer<TInput, TOutput>
{
    #region Private Fields
    private readonly List<Thread> _threadPool = [];
    #endregion

    #region Constructors
    /// <inheritdoc/>
    public ThreadBasedParallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
        int degreeOfParallelism, long totalAmount, int skip = 0, int maxDegreeOfParallelism = 200)
        : base(workItems, workFunction, degreeOfParallelism, totalAmount, skip, maxDegreeOfParallelism)
    {

    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override async Task Start()
    {
        await base.Start();

        Stopwatch.Restart();
        Status = ParallelizerStatus.Running;
        _ = Task.Run(Run).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task Pause()
    {
        await base.Pause();

        Status = ParallelizerStatus.Pausing;
        await WaitCurrentWorkCompletion();
        Status = ParallelizerStatus.Paused;
        Stopwatch.Stop();
    }

    /// <inheritdoc/>
    public override async Task Resume()
    {
        await base.Resume();

        Status = ParallelizerStatus.Running;
        Stopwatch.Start();
    }

    /// <inheritdoc/>
    public override async Task Stop()
    {
        await base.Stop();

        Status = ParallelizerStatus.Stopping;
        await SoftCts.CancelAsync();
        await WaitCompletion().ConfigureAwait(false);
        Stopwatch.Stop();
    }

    /// <inheritdoc/>
    public override async Task Abort()
    {
        await base.Abort();

        Status = ParallelizerStatus.Stopping;
        await HardCts.CancelAsync();
        await SoftCts.CancelAsync();
        await WaitCompletion().ConfigureAwait(false);
        Stopwatch.Stop();
    }

    /// <inheritdoc/>
    public override async Task ChangeDegreeOfParallelism(int newValue)
    {
        await base.ChangeDegreeOfParallelism(newValue);

        DegreeOfParallelism = newValue;
    }
    #endregion

    #region Private Methods
    // Run is executed in fire and forget mode (not awaited)
    private async void Run()
    {
        // Skip the items
        using var items = WorkItems.Skip(Skip).GetEnumerator();

        while (items.MoveNext())
        {
            WAIT:

            // If we paused, stay idle
            if (Status is ParallelizerStatus.Pausing or ParallelizerStatus.Paused)
            {
                await Task.Delay(1000);
                goto WAIT;
            }

            // If we canceled the loop
            if (SoftCts.IsCancellationRequested)
            {
                break;
            }

            // If we haven't filled the thread pool yet, start a new thread
            // (e.g. if we're at the beginning or the increased the DOP)
            if (_threadPool.Count < DegreeOfParallelism)
            {
                StartNewThread(items.Current);
            }
            // Otherwise if we already filled the thread pool
            else
            {
                // If we exceeded the CPM threshold, update CPM and go back to waiting
                if (IsCpmLimited())
                {
                    UpdateCpm();
                    await Task.Delay(100);
                    goto WAIT;
                }

                // Search for the first idle thread
                var firstFree = _threadPool.FirstOrDefault(t => !t.IsAlive);

                // If there is none, go back to waiting
                if (firstFree == null)
                {
                    await Task.Delay(100);
                    goto WAIT;
                }

                // Otherwise remove it
                _threadPool.Remove(firstFree);

                // If there's space for a new thread, start it
                if (_threadPool.Count < DegreeOfParallelism)
                {
                    StartNewThread(items.Current);
                }
                // Otherwise go back to waiting
                else
                {
                    await Task.Delay(100);
                    goto WAIT;
                }
            }
        }

        // Wait until ongoing threads finish
        await WaitCurrentWorkCompletion();

        OnCompleted();
        Status = ParallelizerStatus.Idle;
        HardCts.Dispose();
        SoftCts.Dispose();
        Stopwatch.Stop();
    }

    // Creates and starts a thread, given a work item
    private void StartNewThread(TInput item)
    {
        var thread = new Thread(ThreadWork);
        _threadPool.Add(thread);
        thread.Start(item);
    }

    // Sync method to be passed to a thread
    private void ThreadWork(object? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        TaskFunction((TInput)input).Wait();
    }

    // Wait until the current round is over (if we didn't cancel, it's the last one)
    private async Task WaitCurrentWorkCompletion()
    {
        while (_threadPool.Any(t => t.IsAlive))
        {
            await Task.Delay(100);
        }
    }
    #endregion
}
