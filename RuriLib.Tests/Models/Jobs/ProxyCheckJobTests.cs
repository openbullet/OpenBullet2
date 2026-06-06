using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies;
using RuriLib.Parallelization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Jobs;

public class ProxyCheckJobTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public void Defaults_AreSafe()
    {
        var job = CreateJob();

        Assert.Equal(1, job.Bots);
        Assert.Equal("https://google.com", job.Url);
        Assert.Equal("title>Google", job.SuccessKey);
        Assert.Null(job.Proxies);
        Assert.Null(job.ProxyOutput);
        Assert.Null(job.GeoProvider);
        Assert.Equal(TimeSpan.FromSeconds(10), job.Timeout);
        Assert.True(job.UseProxyJudge);
    }

    [Fact]
    public async Task Start_WithoutProxies_Throws()
    {
        var job = CreateJob();
        job.ProxyOutput = new NullProxyCheckOutput();

        var exception = await Assert.ThrowsAsync<NullReferenceException>(() => job.Start(TestCancellationToken));

        Assert.Equal("The proxy list cannot be null", exception.Message);
        Assert.Equal(JobLastRunOutcome.Failed, job.LastRunOutcome);
    }

    [Fact]
    public async Task Start_WithoutOutput_Throws()
    {
        var job = CreateJob();
        job.Proxies = new List<Proxy>();

        var exception = await Assert.ThrowsAsync<NullReferenceException>(() => job.Start(TestCancellationToken));

        Assert.Equal("The proxy check output cannot be null", exception.Message);
        Assert.Equal(JobLastRunOutcome.Failed, job.LastRunOutcome);
    }

    [Fact]
    public async Task Start_WhenRunCompletes_SetsLastRunOutcomeCompleted()
    {
        var job = CreateJob();
        job.StartCondition = new ImmediateStartCondition();
        job.ProxyOutput = new NullProxyCheckOutput();
        job.UseProxyJudge = false;
        job.Timeout = TimeSpan.FromMilliseconds(50);
        job.SuccessKey = "example";
        job.Proxies =
        [
            new Proxy("127.0.0.1", 1)
        ];

        await job.Start(TestCancellationToken);
        await WaitUntilIdleAsync(job);
        await WaitUntilOutcomeAsync(job);

        Assert.Equal(JobStatus.Idle, job.Status);
        Assert.Equal(JobLastRunOutcome.Completed, job.LastRunOutcome);
    }

    [Fact]
    public void PropagateCompleted_DoesNotOverrideOutcome_WhenProgressIsNotComplete()
    {
        var job = CreateJob();

        typeof(ProxyCheckJob)
            .GetField("parallelizer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, new FakeProxyCheckParallelizer(processed: 1, total: 2));

        typeof(Job)
            .GetProperty(nameof(Job.LastRunOutcome))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(job, [JobLastRunOutcome.Aborted]);

        typeof(ProxyCheckJob)
            .GetMethod("PropagateCompleted", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(job, [null, EventArgs.Empty]);

        Assert.Equal(JobLastRunOutcome.Aborted, job.LastRunOutcome);
    }

    [Fact]
    public void StatusChanged_ToIdle_SetsStoppedOutcome_WhenPendingOutcomeExists()
    {
        var job = CreateJob();

        typeof(ProxyCheckJob)
            .GetField("parallelizer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, new FakeProxyCheckParallelizer(processed: 1, total: 2));

        typeof(ProxyCheckJob)
            .GetField("pendingLastRunOutcome", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, JobLastRunOutcome.Stopped);

        typeof(ProxyCheckJob)
            .GetMethod("StatusChanged", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(job, [null, ParallelizerStatus.Idle]);

        Assert.Equal(JobStatus.Idle, job.Status);
        Assert.Equal(JobLastRunOutcome.Stopped, job.LastRunOutcome);
    }

    [Fact]
    public async Task ChangeBots_WithoutParallelizer_StillUpdatesBots()
    {
        var job = CreateJob();

        await job.ChangeBots(5);

        Assert.Equal(5, job.Bots);
    }

    [Fact]
    public void AzenvProxyJudge_Transparent_WhenForwardedIpIsPresent()
    {
        var judge = new AzenvProxyJudge();
        const string content = """
            HTTP_X_FORWARDED_FOR = 198.51.100.10
            HTTP_VIA = 1.1 proxy
            REMOTE_ADDR = 203.0.113.5
            """;

        var success = judge.TryClassify(content, out var quality);

        Assert.True(success);
        Assert.Equal(ProxyQuality.Transparent, quality);
    }

    [Fact]
    public void AzenvProxyJudge_Anonymous_WhenViaIsPresentWithoutIpLeak()
    {
        var judge = new AzenvProxyJudge();
        const string content = """
            HTTP_VIA = 1.1 proxy
            REMOTE_ADDR = 203.0.113.5
            """;

        var success = judge.TryClassify(content, out var quality);

        Assert.True(success);
        Assert.Equal(ProxyQuality.Anonymous, quality);
    }

    [Fact]
    public void AzenvProxyJudge_Elite_WhenOnlyRemoteAddrIsPresent()
    {
        var judge = new AzenvProxyJudge();
        const string content = """
            HTTP_HOST = azenv.net
            REMOTE_ADDR = 203.0.113.5
            REQUEST_METHOD = GET
            """;

        var success = judge.TryClassify(content, out var quality);

        Assert.True(success);
        Assert.Equal(ProxyQuality.Elite, quality);
    }

    private static ProxyCheckJob CreateJob()
        => new(CreateSettingsService(), CreatePluginRepository());

    private static global::RuriLib.Services.RuriLibSettingsService CreateSettingsService()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-proxycheck-settings-{Guid.NewGuid():N}"));

    private static global::RuriLib.Services.PluginRepository CreatePluginRepository()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-proxycheck-plugins-{Guid.NewGuid():N}"));

    private static async Task WaitUntilIdleAsync(ProxyCheckJob job)
    {
        if (job.Status == JobStatus.Idle)
        {
            return;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnStatusChanged(object? _, JobStatus status)
        {
            if (status == JobStatus.Idle)
            {
                tcs.TrySetResult();
            }
        }

        job.OnStatusChanged += OnStatusChanged;

        if (job.Status == JobStatus.Idle)
        {
            job.OnStatusChanged -= OnStatusChanged;
            return;
        }

        using var registration = TestCancellationToken.Register(() => tcs.TrySetCanceled(TestCancellationToken));

        try
        {
            await tcs.Task;
        }
        finally
        {
            job.OnStatusChanged -= OnStatusChanged;
        }

        Assert.Equal(JobStatus.Idle, job.Status);
    }

    private static async Task WaitUntilOutcomeAsync(ProxyCheckJob job)
    {
        if (job.LastRunOutcome != JobLastRunOutcome.None)
        {
            return;
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

        while (job.LastRunOutcome == JobLastRunOutcome.None)
        {
            await Task.Delay(10, timeoutCts.Token);
        }
    }

    private sealed class NullProxyCheckOutput : IProxyCheckOutput
    {
        public Task StoreAsync(Proxy proxy) => Task.CompletedTask;
    }

    private sealed class ImmediateStartCondition : StartCondition
    {
        public override bool Verify(Job job) => true;
    }

    private sealed class FakeProxyCheckParallelizer
        : global::RuriLib.Parallelization.Parallelizer<ProxyCheckInput, Proxy>
    {
        public FakeProxyCheckParallelizer(int processed, long total)
            : base(
                Array.Empty<ProxyCheckInput>(),
                static (_, _) => Task.FromResult<Proxy>(null!),
                degreeOfParallelism: 1,
                totalAmount: total)
        {
            Processed = processed;
        }
    }
}
