using RuriLib.Parallelization.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Parallelization.Tests;

public class ParallelizerTests
{
    private readonly Func<int, CancellationToken, Task<bool>> _parityCheck
        = (number, _) => Task.FromResult(number % 2 == 0);
        
    private readonly Func<int, CancellationToken, Task<bool>> _longTask
        = async (_, _) => { await Task.Delay(100); return true; };

    private const ParallelizerType _type = ParallelizerType.TaskBased;
    private int _progressCount;
    private bool _lastResult;
    private bool _completedFlag;
    private Exception? _lastException;

    private void OnProgress(object? sender, float value) => _progressCount++;
    
    private void OnResult(object? sender, ResultDetails<int, bool> value) => _lastResult = value.Result;
    
    private void OnCompleted(object? sender, EventArgs e) => _completedFlag = true;
    
    private void OnException(object? sender, Exception ex) => _lastException = ex;
    
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

        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(100, _progressCount);
        Assert.True(_completedFlag);
        Assert.Null(_lastException);
        Assert.True(_lastResult);
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

        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);

        var elapsed = parallelizer.Elapsed;
        await Task.Delay(1000, cts.Token);
        Assert.Equal(elapsed, parallelizer.Elapsed);
    }

    [Fact]
    public async Task Run_LongTasks_StopBeforeCompletion()
    {
        // In theory this should take 1000 * 100 / 10 = 10.000 ms = 10 seconds
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 1000),
            workFunction: _longTask,
            degreeOfParallelism: 10,
            totalAmount: 1000,
            skip: 0);

        _progressCount = 0;
        _completedFlag = false;
        _lastException = null;
        parallelizer.ProgressChanged += OnProgress;
        parallelizer.Completed += OnCompleted;
        parallelizer.Error += OnException;

        await parallelizer.Start();
        await Task.Delay(250);

        await parallelizer.Stop();

        Assert.InRange(_progressCount, 10, 50);
        Assert.True(_completedFlag);
        Assert.Null(_lastException);
    }

    [Fact]
    public async Task Run_LongTasks_AbortBeforeCompletion()
    {
        // In theory this should take 1000 * 100 / 10 = 10.000 ms = 10 seconds
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 1000),
            workFunction: _longTask,
            degreeOfParallelism: 10,
            totalAmount: 1000,
            skip: 0);
            
        _progressCount = 0;
        _completedFlag = false;
        _lastException = null;
        parallelizer.ProgressChanged += OnProgress;
        parallelizer.Completed += OnCompleted;
        parallelizer.Error += OnException;

        await parallelizer.Start();
        await Task.Delay(250);

        await parallelizer.Abort();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);

        Assert.InRange(_progressCount, 10, 50);
        Assert.True(_completedFlag);
        Assert.Null(_lastException);
    }

    [Fact]
    public async Task Run_IncreaseConcurrentThreads_CompleteFaster()
    {
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 10),
            workFunction: _longTask,
            degreeOfParallelism: 1,
            totalAmount: 10,
            skip: 0);
            
        var stopwatch = new Stopwatch();

        // Start with 1 concurrent task
        stopwatch.Start();
        await parallelizer.Start();

        // Wait for 2 rounds to fully complete
        await Task.Delay(250);

        // Release 3 more slots
        await parallelizer.ChangeDegreeOfParallelism(4);

        // Wait until finished
        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);
        stopwatch.Stop();

        // Make sure it took less than 10 * 100 ms (let's say 800)
        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 800);
    }

    [Fact]
    public async Task Run_DecreaseConcurrentThreads_CompleteSlower()
    {
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, 12),
            workFunction: _longTask,
            degreeOfParallelism: 3,
            totalAmount: 12,
            skip: 0);

        var stopwatch = new Stopwatch();

        // Start with 3 concurrent tasks
        stopwatch.Start();
        await parallelizer.Start();

        // Wait for 1 round to complete (a.k.a 3 completed since there are 3 concurrent threads)
        await Task.Delay(150);

        // Remove 2 slots
        await parallelizer.ChangeDegreeOfParallelism(1);

        // Wait until finished
        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);
        stopwatch.Stop();

        // Make sure it took more than 12 * 100 / 3 = 400 ms (we'll say 600 to make sure)
        Assert.True(stopwatch.ElapsedMilliseconds > 600);
    }

    [Fact]
    public async Task Run_PauseAndResume_CompleteAll()
    {
        const int count = 10;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: _longTask,
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
        await Task.Delay(150);
        await parallelizer.Pause();

        // Make sure it's actually paused and nothing is going on
        var progress = _progressCount;
        await Task.Delay(1000);
        Assert.Equal(progress, _progressCount);

        await parallelizer.Resume();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        await parallelizer.WaitCompletion(cts.Token);

        Assert.Equal(count, _progressCount);
        Assert.True(_completedFlag);
        Assert.Null(_lastException);
    }

    [Fact]
    public async Task Run_Pause_StopwatchStops()
    {
        const int count = 10;
        var parallelizer = ParallelizerFactory<int, bool>.Create(
            type: _type,
            workItems: Enumerable.Range(1, count),
            workFunction: _longTask,
            degreeOfParallelism: 1,
            totalAmount: count,
            skip: 0);

        await parallelizer.Start();
        await Task.Delay(150);
        await parallelizer.Pause();

        var elapsed = parallelizer.Elapsed;
        await Task.Delay(1000);
        Assert.Equal(elapsed, parallelizer.Elapsed);

        await parallelizer.Abort();
    }
}
