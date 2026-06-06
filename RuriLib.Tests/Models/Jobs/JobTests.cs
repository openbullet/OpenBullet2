using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Jobs;

public class JobTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    private static readonly global::RuriLib.Services.PluginRepository PluginRepository = CreatePluginRepository();

    [Fact]
    public void SkipWait_BeforeStart_DoesNotThrow()
    {
        var job = CreateJob();

        var exception = Record.Exception(job.SkipWait);

        Assert.Null(exception);
    }

    [Fact]
    public async Task Start_SetsElapsedBaseline()
    {
        var job = CreateJob();
        job.StartCondition = new ImmediateStartCondition();

        await job.Start(TestCancellationToken);

        Assert.True(job.Elapsed >= TimeSpan.Zero);
    }

    [Fact]
    public void TinyDerivedJobs_AcceptNullLogger()
    {
        var settings = CreateSettingsService();
        var pluginRepo = PluginRepository;

        var puppeteer = new PuppeteerUnitTestJob(settings, pluginRepo);
        var rip = new RipJob(settings, pluginRepo);
        var spider = new SpiderJob(settings, pluginRepo);

        Assert.NotNull(puppeteer);
        Assert.NotNull(rip);
        Assert.NotNull(spider);
    }

    private static TestJob CreateJob()
        => new(CreateSettingsService(), PluginRepository);

    private static global::RuriLib.Services.RuriLibSettingsService CreateSettingsService()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-job-tests-settings-{Guid.NewGuid():N}"));

    private static global::RuriLib.Services.PluginRepository CreatePluginRepository()
        => (global::RuriLib.Services.PluginRepository)RuntimeHelpers
            .GetUninitializedObject(typeof(global::RuriLib.Services.PluginRepository));

    private sealed class TestJob(global::RuriLib.Services.RuriLibSettingsService settings,
        global::RuriLib.Services.PluginRepository pluginRepo) : Job(settings, pluginRepo)
    {
    }

    private sealed class ImmediateStartCondition : StartCondition
    {
        public override bool Verify(Job job) => true;
    }
}
