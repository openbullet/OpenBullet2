using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.Threading
{
    public class TaskManager<TInput, TOutput>
    {
        private TaskManagerStatus status = TaskManagerStatus.Idle;
        public TaskManagerStatus Status
        {
            get => status;
            private set
            {
                status = value;
                OnStatusChanged?.Invoke(this, status);
            }
        }

        public float Progress { get; private set; } = 0;
        public int CPM { get; private set; } = 0;
        public DateTime StartTime { get; private set; }
        public DateTime ETA => CPM > 0 ? StartTime + TimeSpan.FromMinutes((totalAmount * (1 - Progress)) / CPM) : DateTime.MaxValue;
        public TimeSpan Elapsed => DateTime.Now - StartTime;
        public TimeSpan Remaining => ETA - DateTime.Now;

        private int concurrentTasks;
        private readonly int maximumConcurrentTasks;
        private readonly IEnumerable<TInput> workItems;
        private readonly Func<TInput, CancellationToken, Task<TOutput>> workFunction;
        private readonly Func<TInput, Task> taskFunction;
        ConcurrentQueue<TInput> queue;
        private readonly int totalAmount;
        private readonly int skip;
        private int current;
        private List<DateTime> checkedTimestamps = new List<DateTime>();
        private readonly object cpmLock = new object();
        private int BatchSize => maximumConcurrentTasks * 2;

        public event EventHandler<ErrorDetails<TInput>> OnTaskError;
        public event EventHandler<Exception> OnError;
        public event EventHandler<ResultDetails<TInput, TOutput>> OnResult;
        public event EventHandler<float> OnProgress;
        public event EventHandler OnCompleted;
        public event EventHandler<TaskManagerStatus> OnStatusChanged;

        private SemaphoreSlim semaphore;

        private CancellationTokenSource cancellationTokenSource;      // Use this to cancel the WaitAll
        private CancellationTokenSource innerCancellationTokenSource; // Use this to soft cancel tasks
        private CancellationTokenSource workCancellationTokenSource;  // Use this to hard cancel tasks
        private int savedConcurrentTasks;
        private bool concurrentTasksDecreaseRequested;

        public TaskManager(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
            int concurrentTasks, int totalAmount, int skip = 0, int maximumConcurrentTasks = 200)
        {
            if (concurrentTasks < 1)
                throw new ArgumentException("There must be at least 1 concurrent task");

            if (skip >= totalAmount)
                throw new ArgumentException("The skip must be less than the total amount");

            if (concurrentTasks > maximumConcurrentTasks)
                throw new ArgumentException($"The concurrent tasks cannot be higher than the maximum ({maximumConcurrentTasks})");

            this.workItems = workItems ?? throw new ArgumentNullException(nameof(workItems));
            this.workFunction = workFunction ?? throw new ArgumentNullException(nameof(workFunction));
            this.totalAmount = totalAmount;
            this.concurrentTasks = concurrentTasks;
            this.maximumConcurrentTasks = maximumConcurrentTasks;
            this.skip = skip;

            // Assign the task function
            taskFunction = new Func<TInput, Task>(async item =>
            {
                if (innerCancellationTokenSource.IsCancellationRequested)
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

                    // Update CPM (only 1 task can enter)
                    if (System.Threading.Monitor.TryEnter(cpmLock))
                    {
                        try
                        {
                            var now = DateTime.Now;
                            checkedTimestamps = checkedTimestamps.Where(t => (now - t).TotalSeconds < 60).ToList();
                            CPM = checkedTimestamps.Count;
                        }
                        finally
                        {
                            System.Threading.Monitor.Exit(cpmLock);
                        }
                    }

                    semaphore.Release();
                }
            });
        }

        public Task Start()
        {
            if (Status != TaskManagerStatus.Idle)
                throw new RequiredStatusException(TaskManagerStatus.Idle, Status);

            checkedTimestamps.Clear();
            StartTime = DateTime.Now;

            Status = TaskManagerStatus.Running;
            Task.Run(() => Run()).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public async Task Pause()
        {
            if (Status != TaskManagerStatus.Running)
                throw new RequiredStatusException(TaskManagerStatus.Running, Status);

            Status = TaskManagerStatus.Pausing;
            savedConcurrentTasks = concurrentTasks;
            await SetConcurrentTasks(0).ConfigureAwait(false);
            Status = TaskManagerStatus.Paused;
        }

        public async Task Resume()
        {
            if (Status != TaskManagerStatus.Paused)
                throw new RequiredStatusException(TaskManagerStatus.Paused, Status);

            Status = TaskManagerStatus.Resuming;
            await SetConcurrentTasks(savedConcurrentTasks).ConfigureAwait(false);
            Status = TaskManagerStatus.Running;
        }

        public async Task Stop()
        {
            if (Status != TaskManagerStatus.Running && Status != TaskManagerStatus.Paused)
                throw new RequiredStatusException(new TaskManagerStatus[] { TaskManagerStatus.Running, TaskManagerStatus.Paused }, Status);

            Status = TaskManagerStatus.Stopping;
            innerCancellationTokenSource.Cancel();
            await WaitCompletion().ConfigureAwait(false);
        }

        public Task Abort()
        {
            if (Status != TaskManagerStatus.Running && Status != TaskManagerStatus.Paused && Status != TaskManagerStatus.Stopping)
                throw new RequiredStatusException(new TaskManagerStatus[] 
                { TaskManagerStatus.Running, TaskManagerStatus.Paused, TaskManagerStatus.Stopping}, Status);

            Status = TaskManagerStatus.Stopping;

            if (!innerCancellationTokenSource.IsCancellationRequested)
                innerCancellationTokenSource.Cancel();

            workCancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public async Task SetConcurrentTasks(int newValue)
        {
            newValue = Clamp(newValue, 0, maximumConcurrentTasks);

            if (newValue == concurrentTasks)
                return;

            else if (newValue > concurrentTasks)
            {
                semaphore.Release(newValue - concurrentTasks);
            }

            else
            {
                concurrentTasksDecreaseRequested = true;
                for (var i = 0; i < concurrentTasks - newValue; ++i)
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);
                }
                concurrentTasksDecreaseRequested = false;
            }

            concurrentTasks = newValue;
        }

        // Run is executed in fire and forget mode (not awaited)
        private async void Run()
        {
            cancellationTokenSource = new CancellationTokenSource();
            innerCancellationTokenSource = new CancellationTokenSource();
            workCancellationTokenSource = new CancellationTokenSource();
            semaphore = new SemaphoreSlim(concurrentTasks, maximumConcurrentTasks);
            concurrentTasksDecreaseRequested = false;

            // Skip the items
            var items = workItems.Skip(skip).GetEnumerator();

            // Create the queue
            queue = new ConcurrentQueue<TInput>();
            
            // Enqueue the first batch (at most BatchSize items)
            while (queue.Count < BatchSize && items.MoveNext())
            {
                queue.Enqueue(items.Current);
            }

            try
            {
                // While there are items in the queue and we didn't cancel, dequeue one, wait and then
                // queue another task if there are more to queue
                while (!queue.IsEmpty && !cancellationTokenSource.IsCancellationRequested)
                {
                    WAIT:

                    // Wait for the semaphore
                    await semaphore.WaitAsync(innerCancellationTokenSource.Token).ConfigureAwait(false);

                    if (concurrentTasksDecreaseRequested)
                    {
                        semaphore.Release();
                        goto WAIT;
                    }

                    // If the current batch is running out
                    if (queue.Count < maximumConcurrentTasks)
                    {
                        // Queue more items until the BatchSize is reached OR until the enumeration finished
                        while (queue.Count < BatchSize && items.MoveNext())
                        {
                            queue.Enqueue(items.Current);
                        }
                    }

                    // If we can dequeue an item, run it
                    if (queue.TryDequeue(out TInput item))
                    {
                        // The task will release its slot no matter what
                        RunTask(item);
                    }
                    else
                    {
                        semaphore.Release();
                    }
                }

                // Wait for every task to finish unless aborted
                while (Progress < 1 && !cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(100);
                }
            }
            catch (OperationCanceledException)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                    innerCancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
            finally
            {
                OnCompleted?.Invoke(this, EventArgs.Empty);
                Status = TaskManagerStatus.Idle;
            }
        }

        public async Task WaitCompletion()
        {
            while (Status != TaskManagerStatus.Idle) { await Task.Delay(100); }
        }

        public static Task AsTask(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }

        private void RunTask(TInput item)
            => taskFunction.Invoke(item);

        private int Clamp(int a, int min, int max)
        {
            if (a < min) return min;
            else if (a > max) return max;
            else return a;
        }
    }

    public class ErrorDetails<TInput>
    {
        public TInput Item { get; set; }
        public Exception Exception { get; set; }

        public ErrorDetails(TInput item, Exception ex)
        {
            Item = item;
            Exception = ex;
        }
    }

    public class ResultDetails<TInput, TOutput>
    {
        public TInput Item { get; set; }
        public TOutput Result { get; set; }

        public ResultDetails(TInput item, TOutput result)
        {
            Item = item;
            Result = result;
        }
    }
}
