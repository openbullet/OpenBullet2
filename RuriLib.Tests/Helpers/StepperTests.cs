using RuriLib.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Helpers
{
    public class StepperTests
    {
        // This test is temporarily disabled until I figure out why it works perfectly fine on a
        // local machine but fails on half the GitHub Actions runs...
        // [Fact]
        public async Task TryTakeStep_StepperWaiting_Proceed()
        {
            var stepper = new Stepper();
            var sw = new Stopwatch();
            sw.Start();

            // Start a task that would take 1000 ms to complete, but waits
            // for a step twice
            var task = Task.Run(async () =>
            {
                await stepper.WaitForStepAsync();
                await Task.Delay(500);

                await stepper.WaitForStepAsync();
                await Task.Delay(500);
            });

            // Wait 300 ms, then take the first step
            await Task.Delay(300);
            var tookStep = stepper.TryTakeStep();

            Assert.True(tookStep);

            // Wait 800 ms (500 + 300), then take the last step
            await Task.Delay(800);
            tookStep = stepper.TryTakeStep();

            Assert.True(tookStep);

            // Wait until the task completes
            await task.WaitAsync(TimeSpan.FromSeconds(5));

            sw.Stop();
            var elapsed = sw.Elapsed;

            // Make sure the elapsed time is over 1500 ms
            Assert.True(elapsed > TimeSpan.FromMilliseconds(1500));
        }
    }
}
