using RuriLib.Models.Jobs.Threading;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Jobs.Threading
{
    public class TaskManagerTests
    {
        Func<int, CancellationToken, Task<bool>> parityCheck = new Func<int, CancellationToken, Task<bool>>
            ((number, token) => Task.FromResult(number % 2 == 0));

        Func<int, CancellationToken, Task<bool>> longTask = new Func<int, CancellationToken, Task<bool>>
            (async (number, token) => { await Task.Delay(100); return true; });

        int progressCount;
        bool completedFlag;
        bool lastResult;
        Exception lastException;

        [Fact]
        public async Task Run_FewInstantTasks_CompleteAndCall()
        {
            var count = 100;
            var manager = new TaskManager<int, bool>(Enumerable.Range(1, count), parityCheck, 10, count, 0);

            progressCount = 0;
            completedFlag = false;
            lastException = null;
            manager.OnProgress += OnProgress;
            manager.OnResult += OnResult;
            manager.OnCompleted += OnCompleted;
            manager.OnError += OnException;

            await manager.Start();
            await manager.WaitCompletion();

            Assert.Equal(100, progressCount);
            Assert.True(completedFlag);
            Assert.True(lastResult);
            Assert.Null(lastException);
        }

        private void OnProgress(object sender, float value) { progressCount++; }
        private void OnResult(object sender, ResultDetails<int, bool> value) { lastResult = value.Result; }
        private void OnCompleted(object sender, EventArgs e) { completedFlag = true; }
        private void OnException(object sender, Exception ex) { lastException = ex; }

        [Fact]
        public async Task Run_IncreaseConcurrentThreads_CompleteFaster()
        {
            var manager = new TaskManager<int, bool>(Enumerable.Range(1, 10), longTask, 1, 10, 0);
            var stopwatch = new Stopwatch();

            // Start with 1 concurrent task
            stopwatch.Start();
            await manager.Start();

            // Wait for 2 rounds to fully complete
            await Task.Delay(250);

            // Release 3 more slots
            await manager.SetConcurrentTasks(4);

            // Wait until finished
            await manager.WaitCompletion();
            stopwatch.Stop();

            // Make sure it took less than 10 * 100 ms (let's say 800)
            Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 800);
        }

        [Fact]
        public async Task Run_DecreaseConcurrentThreads_CompleteSlower()
        {
            var manager = new TaskManager<int, bool>(Enumerable.Range(1, 12), longTask, 3, 12, 0);
            var stopwatch = new Stopwatch();

            // Start with 3 concurrent tasks
            stopwatch.Start();
            await manager.Start();

            // Wait for 1 round to complete (a.k.a 3 completed since there are 3 concurrent threads)
            await Task.Delay(150);

            // Remove 2 slots
            await manager.SetConcurrentTasks(1);

            // Wait until finished
            await manager.WaitCompletion();
            stopwatch.Stop();

            // Make sure it took more than 12 * 100 / 3 = 400 ms (we'll say 600 to make sure)
            Assert.True(stopwatch.ElapsedMilliseconds > 600);
        }

        [Fact]
        public async Task Run_CancelBeforeCompletion_Stop()
        {
            // In theory this should take 1000 * 100 / 10 = 10.000 ms = 10 seconds
            var manager = new TaskManager<int, bool>(Enumerable.Range(1, 1000), longTask, 10, 1000, 0);
            var stopwatch = new Stopwatch();

            progressCount = 0;
            completedFlag = false;
            lastException = null;
            manager.OnProgress += OnProgress;
            manager.OnCompleted += OnCompleted;
            manager.OnError += OnException;

            stopwatch.Start();
            await manager.Start();
            await Task.Delay(250);
            await manager.Stop();
            await manager.WaitCompletion();
            stopwatch.Stop();

            Assert.InRange(progressCount, 10, 50);
            Assert.True(completedFlag);
            Assert.Null(lastException);

            // Make sure it took around 250 (we'll say 1000 to be sure)
            Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 1000);
        }

        [Fact]
        public async Task Run_PauseAndResume_CompleteAll()
        {
            var count = 10;
            var manager = new TaskManager<int, bool>(Enumerable.Range(1, count), longTask, 1, count, 0);

            progressCount = 0;
            completedFlag = false;
            lastException = null;
            manager.OnProgress += OnProgress;
            manager.OnResult += OnResult;
            manager.OnCompleted += OnCompleted;
            manager.OnError += OnException;

            await manager.Start();
            await Task.Delay(150);
            await manager.Pause();

            // Make sure it's actually paused and nothing is going on
            var progress = progressCount;
            await Task.Delay(1000);
            Assert.Equal(progress, progressCount);

            await manager.Resume();
            await manager.WaitCompletion();

            Assert.Equal(count, progressCount);
            Assert.True(completedFlag);
            Assert.Null(lastException);
        }
    }
}
