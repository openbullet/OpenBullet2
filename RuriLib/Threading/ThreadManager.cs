using RuriLib.Threading.Exceptions;
using RuriLib.Threading.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Threading
{
    /// <summary>
    /// Manages the parallel execution of a set of operations, while supporting
    /// pausing, soft and hard aborting, and event notifications.
    /// </summary>
    /// <typeparam name="TInput">The type of data that will be passed to each operation.</typeparam>
    /// <typeparam name="TOutput">The type of data that the operation returns.</typeparam>
    public class ThreadManager<TInput, TOutput> : IThreadManager<TInput, TOutput>
    {
        #region Public Properties
        /// <summary>Retrieves the current status of the <see cref="ThreadManager{TInput, TOutput}"/>.</summary>
        public ThreadManagerStatus Status
        {
            get => status;
            private set
            {
                status = value;
                OnStatusChanged?.Invoke(this, status);
            }
        }

        /// <summary>The progress (from 0 to 1) of the execution.</summary>
        public float Progress { get; private set; } = 0;

        /// <summary>The number of completed operations in the last minute.</summary>
        public int CPM { get; private set; } = 0;

        /// <summary>The time when the execution started.</summary>
        public DateTime StartTime { get; private set; }

        /// <summary>The estimated time of arrival.</summary>
        public DateTime ETA => CPM > 0 ? StartTime + TimeSpan.FromMinutes((totalAmount * (1 - Progress)) / CPM) : DateTime.MaxValue;

        /// <summary>The time elapsed since the start of the execution.</summary>
        public TimeSpan Elapsed => DateTime.Now - StartTime;

        /// <summary>The estimated remaining time until the end of the execution.</summary>
        public TimeSpan Remaining => ETA - DateTime.Now;
        #endregion

        #region Private Fields
        private ThreadManagerStatus status = ThreadManagerStatus.Idle;
        private int parallelThreads;
        private readonly int maximumParallelThreads;
        private readonly IEnumerable<TInput> workItems;
        private readonly Func<TInput, CancellationToken, Task<TOutput>> workFunction;
        private readonly Func<TInput, Task> taskFunction;
        private readonly int totalAmount;
        private readonly int skip;
        private int current;
        private List<DateTime> checkedTimestamps = new();
        private readonly object cpmLock = new();
        private int BatchSize => maximumParallelThreads * 2;

        public List<Thread> threads = new();

        // Cancel loop + work to abort
        // Cancel loop to soft abort
        private CancellationTokenSource loopCancellationTokenSource;
        private CancellationTokenSource workCancellationTokenSource;
        #endregion

        #region Events
        /// <summary>Called when an operation throws an exception.</summary>
        public event EventHandler<ErrorDetails<TInput>> OnTaskError;

        /// <summary>Called when the <see cref="ThreadManager{TInput, TOutput}"/> itself throws an exception.</summary>
        public event EventHandler<Exception> OnError;

        /// <summary>Called when an operation is completed successfully.</summary>
        public event EventHandler<ResultDetails<TInput, TOutput>> OnResult;

        /// <summary>Called when the progress changes.</summary>
        public event EventHandler<float> OnProgress;

        /// <summary>Called when all operations were completed successfully.</summary>
        public event EventHandler OnCompleted;

        /// <summary>Called when <see cref="Status"/> changes.</summary>
        public event EventHandler<ThreadManagerStatus> OnStatusChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the <see cref="ThreadManager{TInput, TOutput}"/>.
        /// </summary>
        /// <param name="workItems">The collection of data to process in parallel</param>
        /// <param name="workFunction">The work function that must be executed on the data</param>
        /// <param name="concurrentTasks">The amount of concurrent tasks that can be started</param>
        /// <param name="totalAmount">The total amount of data that is expected from <paramref name="workItems"/></param>
        /// <param name="skip">The amount of <paramref name="workItems"/> to skip at the beginning</param>
        /// <param name="maximumConcurrentTasks">The maximum amount of <paramref name="concurrentTasks"/> that can be set at any point</param>
        public ThreadManager(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
            int parallelThreads, int totalAmount, int skip = 0, int maximumParallelThreads = 200)
        {
            if (skip >= totalAmount)
                throw new ArgumentException("The skip must be less than the total amount");

            if (parallelThreads < 1 || parallelThreads > maximumParallelThreads)
                throw new ArgumentException($"The number of parallel threads must be between 1 and {maximumParallelThreads}");

            this.workItems = workItems ?? throw new ArgumentNullException(nameof(workItems));
            this.workFunction = workFunction ?? throw new ArgumentNullException(nameof(workFunction));
            this.totalAmount = totalAmount;
            this.parallelThreads = parallelThreads;
            this.maximumParallelThreads = maximumParallelThreads;
            this.skip = skip;

            // Assign the task function
            taskFunction = new Func<TInput, Task>(async item =>
            {
                if (loopCancellationTokenSource.IsCancellationRequested)
                    return;

                // Try to execute the work and report the result
                try
                {
                    var workResult = await workFunction.Invoke(item, workCancellationTokenSource.Token).ConfigureAwait(false);
                    workCancellationTokenSource.Token.ThrowIfCancellationRequested();
                    OnResult?.Invoke(this, new ResultDetails<TInput, TOutput>(item, workResult));
                }
                // Catch and report any exceptions
                catch (Exception ex)
                {
                    OnTaskError?.Invoke(this, new ErrorDetails<TInput>(item, ex));
                }
                // Report the progress, update the CPM and release the semaphore slot
                finally
                {
                    Progress = (float)(++current + skip) / totalAmount;
                    OnProgress?.Invoke(this, Progress);

                    checkedTimestamps.Add(DateTime.Now);

                    // Update CPM (only 1 thread can enter)
                    if (Monitor.TryEnter(cpmLock))
                    {
                        try
                        {
                            var now = DateTime.Now;
                            checkedTimestamps = checkedTimestamps.Where(t => (now - t).TotalSeconds < 60).ToList();
                            CPM = checkedTimestamps.Count;
                        }
                        finally
                        {
                            Monitor.Exit(cpmLock);
                        }
                    }
                }
            });
        }
        #endregion

        #region Public Methods
        public Task Start()
        {
            if (Status != ThreadManagerStatus.Idle)
                throw new RequiredStatusException(ThreadManagerStatus.Idle, Status);

            checkedTimestamps.Clear();
            StartTime = DateTime.Now;

            Status = ThreadManagerStatus.Running;
            Task.Run(() => Run()).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public async Task Pause()
        {
            if (Status != ThreadManagerStatus.Running)
                throw new RequiredStatusException(ThreadManagerStatus.Running, Status);

            Status = ThreadManagerStatus.Pausing;
            await WaitCurrentWorkCompletion();
            Status = ThreadManagerStatus.Paused;
        }

        public Task Resume()
        {
            if (Status != ThreadManagerStatus.Paused)
                throw new RequiredStatusException(ThreadManagerStatus.Paused, Status);

            Status = ThreadManagerStatus.Running;
            return Task.CompletedTask;
        }

        // Cancella il token loop
        public async Task Stop()
        {
            if (Status != ThreadManagerStatus.Running && Status != ThreadManagerStatus.Paused)
                throw new RequiredStatusException(new ThreadManagerStatus[] { ThreadManagerStatus.Running, ThreadManagerStatus.Paused }, Status);

            Status = ThreadManagerStatus.Stopping;
            loopCancellationTokenSource.Cancel();
            await WaitCompletion().ConfigureAwait(false);
        }

        // Cancella i token loop e work
        public Task Abort()
        {
            if (Status != ThreadManagerStatus.Running && Status != ThreadManagerStatus.Paused && Status != ThreadManagerStatus.Stopping)
                throw new RequiredStatusException(new ThreadManagerStatus[]
                { ThreadManagerStatus.Running, ThreadManagerStatus.Paused, ThreadManagerStatus.Stopping}, Status);

            Status = ThreadManagerStatus.Stopping;

            if (!loopCancellationTokenSource.IsCancellationRequested)
                loopCancellationTokenSource.Cancel();

            workCancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public Task SetParallelThreads(int newValue)
        {
            parallelThreads = newValue;
            return Task.CompletedTask;
        }

        public async Task WaitCompletion(CancellationToken cancellationToken = default)
        {
            while (Status != ThreadManagerStatus.Idle)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
            }
        }
        #endregion

        #region Private Methods
        // Run is executed in fire and forget mode (not awaited)
        private async void Run()
        {
            loopCancellationTokenSource = new CancellationTokenSource();
            workCancellationTokenSource = new CancellationTokenSource();

            // Skip the items
            var items = workItems.Skip(skip).GetEnumerator();

            // Itera su tutti gli item
            while (items.MoveNext())
            {
                WAIT:

                // Se abbiamo messo in pausa, resta in idle
                if (Status == ThreadManagerStatus.Pausing || Status == ThreadManagerStatus.Paused)
                {
                    await Task.Delay(1000);
                    goto WAIT;
                }

                // Se vogliamo terminare il loop, interrompilo
                if (loopCancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                // Se non abbiamo ancora spawnato tutti i thread iniziane uno nuovo
                // (es. siamo all'inizio o abbiamo incrementato i thread)
                if (threads.Count < parallelThreads)
                {
                    StartNewThread(items.Current);
                }
                // Se invece abbiamo già tutti i thread
                else
                {
                    // Cerca il primo che ha finito il lavoro
                    var firstFree = threads.FirstOrDefault(t => !t.IsAlive);

                    // Se non ce ne sono, continua il giro
                    if (firstFree == null)
                    {
                        await Task.Delay(100);
                        goto WAIT;
                    }

                    // Se invece c'è, rimuovilo
                    threads.Remove(firstFree);

                    // Se c'è spazio per un nuovo thread, inizialo
                    if (threads.Count < parallelThreads)
                    {
                        StartNewThread(items.Current);
                    }
                    // Se invece non c'è spazio torna ad aspettare
                    else
                    {
                        await Task.Delay(100);
                        goto WAIT;
                    }
                }
            }

            // Aspetta che finiscano i thread in corso
            await WaitCurrentWorkCompletion();

            OnCompleted?.Invoke(this, EventArgs.Empty);
            Status = ThreadManagerStatus.Idle;
        }

        // Crea un thread e lo avvia dato un item di lavoro
        private void StartNewThread(TInput item)
        {
            var thread = new Thread(new ParameterizedThreadStart(ThreadWork));
            threads.Add(thread);
            thread.Start(item);
        }

        // Metodo sincrono da passare a un thread
        private void ThreadWork(object input)
            => taskFunction((TInput)input).Wait();

        // Aspetta che finisca il round corrente (se non abbiamo interrotto, è l'ultimo)
        private async Task WaitCurrentWorkCompletion()
        {
            while (threads.Any(t => t.IsAlive))
            {
                await Task.Delay(100);
            }
        }
        #endregion
    }
}
