using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization;

/// <summary>
/// Parallelizer that uses the Parallel.ForEachAsync function.
/// </summary>
public class ParallelBasedParallelizer<TInput, TOutput> : Parallelizer<TInput, TOutput>
{
    #region Constructors
    /// <inheritdoc/>
    public ParallelBasedParallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
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

        throw new NotSupportedException("This parallelizer does not support pausing");
    }

    /// <inheritdoc/>
    public override async Task Resume()
    {
        await base.Resume().ConfigureAwait(false);

        throw new NotSupportedException("This parallelizer does not support resuming");
    }

    /// <inheritdoc/>
    public override async Task Stop()
    {
        await base.Stop().ConfigureAwait(false);

        throw new NotSupportedException("This parallelizer does not support soft stopping");
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

        if (Status != ParallelizerStatus.Idle)
        {
            throw new NotSupportedException("You cannot change the DoP while this parallelizer is running");
        }
        
        DegreeOfParallelism = newValue;
    }
    #endregion

    #region Private Methods
    // Run is executed in fire and forget mode (not awaited)
    private async void Run()
    {
        // Skip the items
        var items = WorkItems.Skip(Skip);

        try
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = DegreeOfParallelism,
                TaskScheduler = TaskScheduler.Default,
                CancellationToken = HardCts.Token
            };
            await Parallel.ForEachAsync(items, options, async (item, _) =>
            {
                await TaskFunction(item).ConfigureAwait(false);
            });
        }
        catch (TaskCanceledException)
        {
            // Operation aborted, don't throw the error
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
            Stopwatch.Stop();
        }
    }
    #endregion
}
