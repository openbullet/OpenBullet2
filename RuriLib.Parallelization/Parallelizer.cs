using RuriLib.Parallelization.Exceptions;
using RuriLib.Parallelization.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization;

/// <summary>
/// Provides a managed way to execute parallelized work.
/// </summary>
/// <typeparam name="TInput">The type of the workload items</typeparam>
/// <typeparam name="TOutput">The type of the results</typeparam>
public abstract class Parallelizer<TInput, TOutput> : IDisposable
{
    #region Public Fields
    /// <summary>
    /// The maximum value that the degree of parallelism can have when changed through the
    /// <see cref="Parallelizer{TInput, TOutput}.ChangeDegreeOfParallelism(int, CancellationToken)"/> method.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; }

    /// <summary>
    /// The current status of the parallelizer.
    /// </summary>
    public ParallelizerStatus Status
    {
        get
        {
            lock (StatusLock)
            {
                return _status;
            }
        }
        protected set
        {
            SetStatusValue(value);
        }
    }

    /// <summary>
    /// Retrieves the current progress in the interval [0, 1].
    /// The progress is -1 if the manager hasn't been started yet.
    /// </summary>
    public float Progress => (float)(Processed + Skip) / TotalAmount;

    /// <summary>
    /// Retrieves the completed work per minute.
    /// </summary>
    public int CPM { get; protected set; }

    /// <summary>
    /// Sets a maximum threshold for CPM. 0 to disable.
    /// </summary>
    public int CPMLimit { get; set; } = 0;

    /// <summary>
    /// The time when the parallelizer started its work for its last running session.
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    /// The time when the parallelizer finished its work or was stopped (<see langword="null"/> if it hasn't finished
    /// a single session yet).
    /// </summary>
    public DateTime? EndTime { get; private set; }

    /// <summary>
    /// The Estimated Time of Arrival (when the parallelizer is expected to finish all the work).
    /// </summary>
    public DateTime ETA
    {
        get
        {
            var minutes = TotalAmount * (1 - Progress) / CPM;
            return CPM > 0 && minutes < TimeSpan.MaxValue.TotalMinutes
                ? StartTime + TimeSpan.FromMinutes(minutes)
                : DateTime.MaxValue;
        }
    }

    /// <summary>
    /// The time elapsed since the start of the session.
    /// </summary>
    public TimeSpan Elapsed => TimeSpan.FromMilliseconds(Stopwatch.ElapsedMilliseconds);

    /// <summary>
    /// The expected remaining time to finish all the work.
    /// </summary>
    public TimeSpan Remaining => EndTime.HasValue ? TimeSpan.Zero : ETA - DateTime.Now;
    #endregion

    #region Protected Fields
    /// <summary>
    /// The number of items that can be processed concurrently.
    /// </summary>
    protected int DegreeOfParallelism;

    /// <summary>
    /// The items to process.
    /// </summary>
    protected readonly IEnumerable<TInput> WorkItems;

    /// <summary>
    /// The function to process items and get results.
    /// </summary>
    protected readonly Func<TInput, CancellationToken, Task<TOutput>> WorkFunction;

    /// <summary>
    /// The function that turns each input item into an awaitable <see cref="Task"/>.
    /// </summary>
    protected readonly Func<TInput, Task> TaskFunction;

    /// <summary>
    /// The total amount of work items that are expected to be enumerated (for progress calculations).
    /// </summary>
    protected readonly long TotalAmount;

    /// <summary>
    /// The number of items to skip from the start of the collection (to restore previously aborted sessions).
    /// </summary>
    protected readonly int Skip;

    /// <summary>
    /// The current amount of work items that were processed so far.
    /// </summary>
    protected int Processed;

    /// <summary>
    /// The queue of timestamps for CPM calculation.
    /// </summary>
    protected readonly Queue<long> CheckedTimestamps = [];

    /// <summary>
    /// A lock that can be used to update the CPM from a single thread at a time.
    /// </summary>
    protected readonly object CpmLock = new();

    /// <summary>
    /// The stopwatch that calculates the elapsed time.
    /// </summary>
    protected readonly Stopwatch Stopwatch = new();

    /// <summary>
    /// A soft cancellation token. Cancel this for soft AND hard abort.
    /// </summary>
    protected CancellationTokenSource SoftCts = new();

    /// <summary>
    /// A hard cancellation token. Cancel this for hard abort only.
    /// </summary>
    protected CancellationTokenSource HardCts = new();

    /// <summary>
    /// Synchronizes status transitions with lifecycle waiters.
    /// </summary>
    protected readonly object StatusLock = new();
    #endregion

    #region Private Fields
    /// <summary>
    /// The status of the parallelizer.
    /// </summary>
    private ParallelizerStatus _status = ParallelizerStatus.Idle;
    private TaskCompletionSource _completedSignal = CreateCompletedSignal(completed: true);
    #endregion

    #region Events
    /// <summary>Called when an operation throws an exception.</summary>
    public event EventHandler<ErrorDetails<TInput>>? TaskError;

    /// <summary>
    /// Invokes a <see cref="Parallelizer{TInput, TOutput}.TaskError"/> event.
    /// </summary>
    protected virtual void OnTaskError(ErrorDetails<TInput> input) => TaskError?.Invoke(this, input);

    /// <summary>Called when the <see cref="Parallelizer{TInput, TOutput}"/> itself throws an exception.</summary>
    public event EventHandler<Exception>? Error;

    /// <summary>
    /// Invokes a <see cref="Parallelizer{TInput, TOutput}.Error"/> event.
    /// </summary>
    protected virtual void OnError(Exception ex) => Error?.Invoke(this, ex);

    /// <summary>Called when an operation is completed successfully.</summary>
    public event EventHandler<ResultDetails<TInput, TOutput>>? NewResult;

    /// <summary>
    /// Invokes a <see cref="Parallelizer{TInput, TOutput}.NewResult"/> event.
    /// </summary>
    protected virtual void OnNewResult(ResultDetails<TInput, TOutput> result) => NewResult?.Invoke(this, result);

    /// <summary>Called when the progress changes.</summary>
    public event EventHandler<float>? ProgressChanged;

    /// <summary>
    /// Invokes a <see cref="Parallelizer{TInput, TOutput}.ProgressChanged"/> event.
    /// </summary>
    protected virtual void OnProgressChanged(float progress) => ProgressChanged?.Invoke(this, progress);

    /// <summary>Called when the parallelizer run has ended.</summary>
    public event EventHandler? Completed;

    /// <summary>
    /// Invokes a <see cref="Parallelizer{TInput, TOutput}.Completed"/> event.
    /// </summary>
    protected virtual void OnCompleted()
    {
        try
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _completedSignal.TrySetResult();
        }
    }

    /// <summary>Called when <see cref="Status"/> changes.</summary>
    public event EventHandler<ParallelizerStatus>? StatusChanged;

    /// <summary>
    /// Invokes a <see cref="Parallelizer{TInput, TOutput}.StatusChanged"/> event.
    /// </summary>
    protected virtual void OnStatusChanged(ParallelizerStatus newStatus) => StatusChanged?.Invoke(this, newStatus);
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of <see cref="Parallelizer{TInput, TOutput}"/>.
    /// </summary>
    /// <param name="workItems">The collection of data to process in parallel</param>
    /// <param name="workFunction">The work function that must be executed on the data</param>
    /// <param name="degreeOfParallelism">The amount of concurrent tasks that can be started</param>
    /// <param name="totalAmount">The total amount of data that is expected from <paramref name="workItems"/></param>
    /// <param name="skip">The amount of <paramref name="workItems"/> to skip at the beginning</param>
    /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism that can be set</param>
    protected Parallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
        int degreeOfParallelism, long totalAmount, int skip = 0, int maxDegreeOfParallelism = 200)
    {
        if (degreeOfParallelism < 1)
        {
            throw new ArgumentException("The degree of parallelism must be at least 1");
        }

        if (degreeOfParallelism > maxDegreeOfParallelism)
        {
            throw new ArgumentException("The degree of parallelism must not be greater than the maximum degree of parallelism");
        }

        if (skip >= totalAmount)
        {
            throw new ArgumentException("The skip must be less than the total amount");
        }

        this.WorkItems = workItems ?? throw new ArgumentNullException(nameof(workItems));
        this.WorkFunction = workFunction ?? throw new ArgumentNullException(nameof(workFunction));
        this.TotalAmount = totalAmount;
        this.DegreeOfParallelism = degreeOfParallelism;
        this.Skip = skip;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;

        // Assign the task function
        TaskFunction = async item =>
        {
            if (SoftCts.IsCancellationRequested)
            {
                return;
            }

            // Try to execute the work and report the result
            try
            {
                var workResult = await workFunction.Invoke(item, HardCts.Token).ConfigureAwait(false);
                OnNewResult(new ResultDetails<TInput, TOutput>(item, workResult));
                HardCts.Token.ThrowIfCancellationRequested();
            }
            // Catch and report any exceptions
            catch (Exception ex)
            {
                OnTaskError(new ErrorDetails<TInput>(item, ex));
            }
            // Report the progress, update the CPM and release the semaphore slot
            finally
            {
                Interlocked.Increment(ref Processed);
                OnProgressChanged(Progress);

                RecordCpm();
            }
        };
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Starts the execution (without waiting for completion).
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the caller's request before the run is started.</param>
    public virtual Task Start(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequiredStatusException.ThrowIfNot(Status, ParallelizerStatus.Idle);

        Reset();

        return Task.CompletedTask;
    }

    /// <summary>Pauses the execution (waits until the ongoing operations are completed).</summary>
    /// <param name="cancellationToken">A token that cancels the caller's wait for the pause to complete.</param>
    public virtual Task Pause(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequiredStatusException.ThrowIfNot(Status, ParallelizerStatus.Running);

        return Task.CompletedTask;
    }

    /// <summary>Resumes a paused execution.</summary>
    /// <param name="cancellationToken">A token that cancels the caller's request before resume starts.</param>
    public virtual Task Resume(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequiredStatusException.ThrowIfNot(Status, ParallelizerStatus.Paused);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the execution (waits for the current items to finish).
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the caller's wait for stop completion.</param>
    public virtual Task Stop(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequiredStatusException.ThrowIfNot(Status,
        [
            ParallelizerStatus.Running,
            ParallelizerStatus.Paused
        ]);

        EndTime = DateTime.Now;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Aborts the execution without waiting for the current work to finish.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the caller's wait for abort completion.</param>
    public virtual Task Abort(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequiredStatusException.ThrowIfNot(Status,
        [
            ParallelizerStatus.Running,
            ParallelizerStatus.Paused,
            ParallelizerStatus.Stopping,
            ParallelizerStatus.Pausing
        ]);

        EndTime = DateTime.Now;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dynamically changes the degree of parallelism.
    /// </summary>
    /// <param name="newValue">The new degree of parallelism.</param>
    /// <param name="cancellationToken">A token that cancels the caller's wait for the new limit to be reached.</param>
    public virtual Task ChangeDegreeOfParallelism(int newValue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // This can be 0 because we can use 0 dop as a pausing system
        if (newValue < 0 || newValue > MaxDegreeOfParallelism)
        {
            throw new ArgumentException($"Must be within 0 and {MaxDegreeOfParallelism}", nameof(newValue));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// An awaitable handler that completes when the <see cref="Status"/> is <see cref="ParallelizerStatus.Idle"/>.
    /// </summary>
    public async Task WaitCompletion(CancellationToken cancellationToken = default)
    {
        while (Status != ParallelizerStatus.Idle)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }

        await _completedSignal.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Whether the CPM is limited to a certain amount (for throttling purposes).
    /// </summary>
    /// <returns></returns>
    protected bool IsCpmLimited() => GetCpmThrottleDelay() > TimeSpan.Zero;

    /// <summary>
    /// Gets how long the scheduler should wait before starting more work under the current CPM limit.
    /// </summary>
    protected TimeSpan GetCpmThrottleDelay()
    {
        if (CPMLimit <= 0)
        {
            return TimeSpan.Zero;
        }

        lock (CpmLock)
        {
            var now = Environment.TickCount64;
            UpdateCpm(now);

            if (CPM < CPMLimit)
            {
                return TimeSpan.Zero;
            }

            // Conservatively wait until the oldest completion leaves the 60-second CPM window.
            var remainingMilliseconds = CheckedTimestamps.Peek() + 60000 - now;
            return remainingMilliseconds > 0
                ? TimeSpan.FromMilliseconds(remainingMilliseconds)
                : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Records a completed item and updates the CPM (safe to be called from multiple threads).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected void RecordCpm()
    {
        lock (CpmLock)
        {
            var now = Environment.TickCount64;
            CheckedTimestamps.Enqueue(now);
            UpdateCpm(now);
        }
    }

    /// <summary>
    /// Updates the CPM without recording a completed item (safe to be called from multiple threads).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected void UpdateCpm()
    {
        lock (CpmLock)
        {
            UpdateCpm(Environment.TickCount64);
        }
    }

    private void UpdateCpm(long now)
    {
        while (CheckedTimestamps.Count > 0 && now - CheckedTimestamps.Peek() >= 60000)
        {
            CheckedTimestamps.Dequeue();
        }

        CPM = CheckedTimestamps.Count;
    }

    /// <summary>
    /// Updates the status and raises <see cref="StatusChanged"/> after releasing <see cref="StatusLock"/>.
    /// </summary>
    protected void SetStatusValue(ParallelizerStatus status)
    {
        lock (StatusLock)
        {
            _status = status;
        }

        // Event handlers are user code. Invoke them outside StatusLock so handlers can safely
        // query status or wait for completion without blocking the state transition that raised them.
        OnStatusChanged(status);
    }

    /// <summary>
    /// Updates the status if <paramref name="predicate"/> allows the transition, raising
    /// <see cref="StatusChanged"/> after releasing <see cref="StatusLock"/>.
    /// </summary>
    protected bool TrySetStatusValue(ParallelizerStatus status, Func<ParallelizerStatus, bool> predicate)
    {
        var changed = false;

        // Evaluate the transition predicate against the locked backing field, then raise the
        // notification after releasing the lock. This keeps compound transitions atomic without
        // invoking external event handlers while StatusLock is held.
        lock (StatusLock)
        {
            if (predicate(_status))
            {
                _status = status;
                changed = true;
            }
        }

        if (changed)
        {
            OnStatusChanged(status);
        }

        return changed;
    }
    #endregion

    #region Private Methods
    private void Reset()
    {
        StartTime = DateTime.Now;
        EndTime = null;
        Processed = 0;
        CPM = 0;
        CheckedTimestamps.Clear();
        _completedSignal = CreateCompletedSignal();

        SoftCts.Dispose();
        HardCts.Dispose();
        SoftCts = new CancellationTokenSource();
        HardCts = new CancellationTokenSource();
    }

    private static TaskCompletionSource CreateCompletedSignal(bool completed = false)
    {
        // Avoid running WaitCompletion continuations inline while OnCompleted is unwinding.
        var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        if (completed)
        {
            signal.SetResult();
        }

        return signal;
    }
    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        SoftCts.Dispose();
        HardCts.Dispose();
    }
}
