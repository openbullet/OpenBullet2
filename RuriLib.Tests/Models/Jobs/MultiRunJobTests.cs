using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using RuriLib.Logging;
using RuriLib.Parallelization;
using RuriLib.Proxies.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Jobs;

public class MultiRunJobTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;
    private static readonly AsyncLocal<CancellationTokenSource?> currentWorkerCancellationSource = new();

    [Fact]
    public void Defaults_AreSafe()
    {
        var job = CreateJob();

        Assert.Equal(1, job.Bots);
        Assert.Null(job.Config);
        Assert.Null(job.DataPool);
        Assert.Empty(job.ProxySources);
        Assert.Empty(job.HitOutputs);
        Assert.Empty(job.CustomInputsAnswers);
        Assert.Empty(job.CurrentBotDatas);
        Assert.Null(job.Providers);
        Assert.False(job.ShouldUseProxies());
    }

    [Fact]
    public void ShouldUseProxies_UsesConfigSettingsWhenAvailable()
    {
        var job = CreateJob();
        job.Config = new Config
        {
            Id = "test",
            Settings = new ConfigSettings()
        };

        job.Config.Settings.ProxySettings.UseProxies = true;
        job.ProxyMode = JobProxyMode.Default;
        Assert.True(job.ShouldUseProxies());

        job.ProxyMode = JobProxyMode.Off;
        Assert.False(job.ShouldUseProxies());

        job.ProxyMode = JobProxyMode.On;
        Assert.True(job.ShouldUseProxies());
    }

    [Fact]
    public async Task Start_WithoutProviders_ThrowsInvalidOperationException()
    {
        var job = CreateJob();
        job.Config = new Config
        {
            Id = "test",
            Settings = new ConfigSettings()
        };
        job.DataPool = new TestDataPool();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => job.Start(TestCancellationToken));

        Assert.Equal("The Providers cannot be null", exception.Message);
        Assert.Equal(JobLastRunOutcome.Failed, job.LastRunOutcome);
    }

    [Fact]
    public async Task FetchProxiesFromSources_BeforeStart_ThrowsInvalidOperationException()
    {
        var job = CreateJob();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => job.FetchProxiesFromSources(TestCancellationToken));

        Assert.Equal("The job has not been initialized yet", exception.Message);
    }

    [Fact]
    public async Task Start_WithEmptyData_InitializesCurrentBotDatas()
    {
        var settings = CreateSettingsService();
        var job = new MultiRunJob(settings, CreatePluginRepository())
        {
            Bots = 3,
            Providers = new global::RuriLib.Models.Bots.Providers(settings),
            StartCondition = new ImmediateStartCondition(),
            Config = new Config
            {
                Id = "test",
                Mode = ConfigMode.Legacy,
                Settings = new ConfigSettings()
            },
            DataPool = new TestDataPool(["data"], settings.Environment.WordlistTypes[0].Name)
        };

        await job.Start(TestCancellationToken);

        Assert.Equal(3, job.CurrentBotDatas.Length);
    }

    [Fact]
    public async Task Start_WithStartupScript_RaisesStartupLogEntries()
    {
        var settings = CreateSettingsService();
        var entries = new List<BotLoggerEntry>();
        var job = new MultiRunJob(settings, CreatePluginRepository())
        {
            Bots = 1,
            Providers = new global::RuriLib.Models.Bots.Providers(settings),
            StartCondition = new ImmediateStartCondition(),
            Config = new Config
            {
                Id = "test",
                Mode = ConfigMode.CSharp,
                CSharpScript = string.Empty,
                StartupCSharpScript = "data.Logger.Log(\"startup\", LogColors.Tomato);",
                Settings = new ConfigSettings()
            },
            DataPool = new TestDataPool(["data"], settings.Environment.WordlistTypes[0].Name)
        };

        job.OnLogEntry += (_, entry) => entries.Add(entry);

        await job.Start(TestCancellationToken);

        Assert.Contains(entries, e => e.Message == "Executing startup script...");
        Assert.Contains(entries, e => e.Message == "startup" && e.Color == LogColors.Tomato);
        Assert.Contains(entries, e => e.Message == "Executing main script...");
    }

    [Fact]
    public async Task WorkFunction_WhenBadProxyExceptionIsThrown_MarksProxyAsBad()
    {
        var settings = CreateSettingsService();
        var proxy = new Proxy("127.0.0.1", 8000);
        var job = new MultiRunJob(settings, CreatePluginRepository());
        var pool = new ProxyPool([new ListProxySource([proxy])]);
        await pool.ReloadAllAsync(false, TestCancellationToken);

        var configSettings = new ConfigSettings();
        configSettings.ProxySettings.UseProxies = true;

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        currentWorkerCancellationSource.Value = cts;

        try
        {
            var input = new MultiRunInput
            {
                Job = job,
                ProxyPool = pool,
                BotData = new global::RuriLib.Models.Bots.BotData(
                    new global::RuriLib.Models.Bots.Providers(settings),
                    configSettings,
                    new BotLogger(),
                    new DataLine("data", settings.Environment.WordlistTypes[0]),
                    null,
                    useProxy: true)
                {
                    CancellationToken = cts.Token
                },
                Globals = new System.Dynamic.ExpandoObject(),
                IsDLL = true,
                DLLMethod = typeof(MultiRunJobTests).GetMethod(nameof(ThrowBadProxyAndCancelAsync),
                    BindingFlags.Static | BindingFlags.NonPublic)
            };

            var workFunction = (Func<MultiRunInput, CancellationToken, Task<CheckResult>>)typeof(MultiRunJob)
                .GetField("workFunction", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(job)!;

            await Assert.ThrowsAsync<TaskCanceledException>(() => workFunction(input, cts.Token));
            Assert.Equal(ProxyStatus.Bad, proxy.ProxyStatus);
        }
        finally
        {
            currentWorkerCancellationSource.Value = null;
            cts.Dispose();
            pool.Dispose();
        }
    }

    [Fact]
    public async Task WorkFunction_WhenBadProxyExceptionIsThrown_AndNeverMarkProxiesAsBad_BansProxy()
    {
        var settings = CreateSettingsService();
        var proxy = new Proxy("127.0.0.1", 8000);
        var job = new MultiRunJob(settings, CreatePluginRepository())
        {
            NeverMarkProxiesAsBad = true
        };
        var pool = new ProxyPool([new ListProxySource([proxy])]);
        await pool.ReloadAllAsync(false, TestCancellationToken);

        var configSettings = new ConfigSettings();
        configSettings.ProxySettings.UseProxies = true;

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        currentWorkerCancellationSource.Value = cts;

        try
        {
            var input = new MultiRunInput
            {
                Job = job,
                ProxyPool = pool,
                BotData = new global::RuriLib.Models.Bots.BotData(
                    new global::RuriLib.Models.Bots.Providers(settings),
                    configSettings,
                    new BotLogger(),
                    new DataLine("data", settings.Environment.WordlistTypes[0]),
                    null,
                    useProxy: true)
                {
                    CancellationToken = cts.Token
                },
                Globals = new System.Dynamic.ExpandoObject(),
                IsDLL = true,
                DLLMethod = typeof(MultiRunJobTests).GetMethod(nameof(ThrowBadProxyAndCancelAsync),
                    BindingFlags.Static | BindingFlags.NonPublic)
            };

            var workFunction = (Func<MultiRunInput, CancellationToken, Task<CheckResult>>)typeof(MultiRunJob)
                .GetField("workFunction", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(job)!;

            await Assert.ThrowsAsync<TaskCanceledException>(() => workFunction(input, cts.Token));
            Assert.Equal(ProxyStatus.Banned, proxy.ProxyStatus);
        }
        finally
        {
            currentWorkerCancellationSource.Value = null;
            cts.Dispose();
            pool.Dispose();
        }
    }

    [Fact]
    public void ResetStats_AlsoResetsInvalidCount()
    {
        var job = CreateJob();

        typeof(MultiRunJob)
            .GetField("dataInvalid", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, 5);

        typeof(MultiRunJob)
            .GetMethod("ResetStats", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(job, null);

        Assert.Equal(0, job.DataInvalid);
    }

    [Theory]
    [InlineData(0, 2, 5, 2)]
    [InlineData(2, 1, 5, 3)]
    [InlineData(2, 3, 5, 0)]
    [InlineData(0, 5, 5, 0)]
    public void GetNextSkip_NormalizesCompletedRuns(int currentSkip, int processed, int total, int expected)
    {
        var nextSkip = MultiRunJobCheckpoint.GetNextSkip(currentSkip, processed, total);

        Assert.Equal(expected, nextSkip);
    }

    [Fact]
    public async Task Start_AfterCompletedRun_RestartsFromBeginning()
    {
        var settings = CreateSettingsService();
        var job = new MultiRunJob(settings, CreatePluginRepository())
        {
            Bots = 1,
            Providers = new global::RuriLib.Models.Bots.Providers(settings),
            StartCondition = new ImmediateStartCondition(),
            Config = new Config
            {
                Id = "test",
                Mode = ConfigMode.Legacy,
                Settings = new ConfigSettings()
            },
            DataPool = new TestDataPool(["data"], settings.Environment.WordlistTypes[0].Name)
        };

        await job.Start(TestCancellationToken);
        await WaitUntilIdleAsync(job);
        Assert.Equal(0, job.Skip);
        Assert.Equal(JobLastRunOutcome.Completed, job.LastRunOutcome);

        var exception = await Record.ExceptionAsync(() => job.Start(TestCancellationToken));
        await WaitUntilIdleAsync(job);

        Assert.Null(exception);
        Assert.Equal(0, job.Skip);
        Assert.Equal(JobLastRunOutcome.Completed, job.LastRunOutcome);
    }

    [Fact]
    public void PropagateCompleted_DoesNotOverrideOutcome_WhenProgressIsNotComplete()
    {
        var job = CreateJob();

        typeof(MultiRunJob)
            .GetField("parallelizer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, new FakeMultiRunParallelizer(processed: 1, total: 2));

        typeof(Job)
            .GetProperty(nameof(Job.LastRunOutcome))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(job, [JobLastRunOutcome.Stopped]);

        typeof(MultiRunJob)
            .GetMethod("PropagateCompleted", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(job, [null, EventArgs.Empty]);

        Assert.Equal(JobLastRunOutcome.Stopped, job.LastRunOutcome);
    }

    [Fact]
    public void StatusChanged_ToIdle_SetsStoppedOutcome_WhenPendingOutcomeExists()
    {
        var job = CreateJob();

        typeof(MultiRunJob)
            .GetField("parallelizer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, new FakeMultiRunParallelizer(processed: 1, total: 2));

        typeof(MultiRunJob)
            .GetField("pendingLastRunOutcome", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, JobLastRunOutcome.Stopped);

        typeof(MultiRunJob)
            .GetMethod("StatusChanged", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(job, [null, ParallelizerStatus.Idle]);

        Assert.Equal(JobStatus.Idle, job.Status);
        Assert.Equal(JobLastRunOutcome.Stopped, job.LastRunOutcome);
    }

    [Fact]
    public async Task GetHitsSnapshot_DuringConcurrentWrites_DoesNotThrow()
    {
        var job = CreateJob();
        var hits = job.Hits;
        var hitsLock = typeof(MultiRunJob)
            .GetField("hitsLock", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(job)!;
        using var cts = new CancellationTokenSource();

        var writer = Task.Run(async () =>
        {
            var index = 0;

            while (!cts.Token.IsCancellationRequested)
            {
                lock (hitsLock)
                {
                    hits.Add(CreateHit(index++));
                }

                await Task.Yield();
            }
        }, TestCancellationToken);

        for (var i = 0; i < 200; i++)
        {
            var exception = Record.Exception(() => job.GetHitsSnapshot());
            Assert.Null(exception);
            await Task.Yield();
        }

        await cts.CancelAsync();
        await writer.WaitAsync(TestCancellationToken);
    }

    [Fact]
    public void FindHit_ReturnsMatchingHit()
    {
        var job = CreateJob();
        var hit = CreateHit(1);
        job.Hits.Add(hit);

        var found = job.FindHit(hit.Id);

        Assert.Same(hit, found);
    }

    private static MultiRunJob CreateJob()
        => new(CreateSettingsService(), CreatePluginRepository());

    private static global::RuriLib.Services.RuriLibSettingsService CreateSettingsService()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-multirun-settings-{Guid.NewGuid():N}"));

    private static global::RuriLib.Services.PluginRepository CreatePluginRepository()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-multirun-plugins-{Guid.NewGuid():N}"));

    private static async Task ThrowBadProxyAndCancelAsync(global::RuriLib.Models.Bots.BotData data,
        dynamic input, dynamic globals, Dictionary<string, object> outputVariables, CancellationToken cancellationToken)
    {
        if (currentWorkerCancellationSource.Value is not null)
        {
            await currentWorkerCancellationSource.Value.CancelAsync();
        }

        throw new BadProxyException("bad proxy");
    }

    private static async Task WaitUntilIdleAsync(MultiRunJob job)
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

    private static global::RuriLib.Models.Hits.Hit CreateHit(int index)
    {
        const string wordlistTypeName = "default";

        return new global::RuriLib.Models.Hits.Hit
        {
            Data = new DataLine(
                $"user{index}:pass{index}",
                new global::RuriLib.Models.Environment.WordlistType { Name = wordlistTypeName }),
            CapturedData = new Dictionary<string, object> { ["token"] = $"abc{index}" },
            Date = DateTime.UtcNow,
            Type = "SUCCESS",
            Config = new Config
            {
                Id = $"cfg-{index}",
                Metadata = new ConfigMetadata { Name = "Config", Category = "Cat" }
            },
            DataPool = new TestDataPool([$"user{index}:pass{index}"], wordlistTypeName)
        };
    }

    private sealed class TestDataPool : DataPool
    {
        public TestDataPool()
        {
        }

        public TestDataPool(string[] data, string wordlistType)
        {
            DataList = data;
            Size = data.Length;
            WordlistType = wordlistType;
        }

        public override void Reload()
        {
        }
    }

    private sealed class ImmediateStartCondition : StartCondition
    {
        public override bool Verify(Job job) => true;
    }

    private sealed class FakeMultiRunParallelizer
        : global::RuriLib.Parallelization.Parallelizer<MultiRunInput, CheckResult>
    {
        public FakeMultiRunParallelizer(int processed, long total)
            : base(
                Array.Empty<MultiRunInput>(),
                static (_, _) => Task.FromResult(default(CheckResult)),
                degreeOfParallelism: 1,
                totalAmount: total)
        {
            Processed = processed;
        }
    }
}
