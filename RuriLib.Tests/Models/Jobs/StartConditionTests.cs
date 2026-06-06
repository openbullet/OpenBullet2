using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Jobs;

public class StartConditionTests
{
    [Fact]
    public void RelativeTimeStartCondition_Verify_UsesJobStartTime()
    {
        var job = CreateJob();
        job.StartTime = DateTime.Now - TimeSpan.FromSeconds(10);
        var condition = new RelativeTimeStartCondition
        {
            StartAfter = TimeSpan.FromSeconds(5)
        };

        Assert.True(condition.Verify(job));
    }

    [Fact]
    public void AbsoluteTimeStartCondition_Verify_UsesStartAt()
    {
        var job = CreateJob();
        var condition = new AbsoluteTimeStartCondition
        {
            StartAt = DateTime.Now - TimeSpan.FromSeconds(1)
        };

        Assert.True(condition.Verify(job));
    }

    [Fact]
    public async Task WaitUntilVerified_WhenCancelled_ThrowsTaskCanceledException()
    {
        var job = CreateJob();
        var condition = new NeverStartCondition();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAsync<TaskCanceledException>(() => condition.WaitUntilVerified(job, cts.Token));
    }

    private static TestJob CreateJob()
        => new(CreateSettingsService(), CreatePluginRepository());

    private static global::RuriLib.Services.RuriLibSettingsService CreateSettingsService()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-startcondition-tests-{Guid.NewGuid():N}"));

    private static global::RuriLib.Services.PluginRepository CreatePluginRepository()
        => (global::RuriLib.Services.PluginRepository)RuntimeHelpers
            .GetUninitializedObject(typeof(global::RuriLib.Services.PluginRepository));

    private sealed class TestJob(global::RuriLib.Services.RuriLibSettingsService settings,
        global::RuriLib.Services.PluginRepository pluginRepo) : Job(settings, pluginRepo)
    {
    }

    private sealed class NeverStartCondition : StartCondition
    {
        public override bool Verify(Job job) => false;
    }
}
