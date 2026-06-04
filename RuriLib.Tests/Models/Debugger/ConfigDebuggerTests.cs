using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Debugger;
using RuriLib.Models.Environment;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Debugger;

public class ConfigDebuggerTests
{
    [Fact]
    public void Constructor_UsesSafeDefaults()
    {
        var config = new Config
        {
            Id = "test"
        };

        using var debugger = new ConfigDebugger(config);

        Assert.Same(config, debugger.Config);
        Assert.NotNull(debugger.Options);
        Assert.NotNull(debugger.Logger);
        Assert.Equal(ConfigDebuggerStatus.Idle, debugger.Status);
    }

    [Fact]
    public void TryTakeStep_WhenNotRunning_ReturnsFalse()
    {
        using var debugger = new ConfigDebugger(new Config { Id = "test" });

        Assert.False(debugger.TryTakeStep());
    }

    [Fact]
    public void Stop_WhenNotRunning_DoesNotThrow()
    {
        using var debugger = new ConfigDebugger(new Config { Id = "test" });

        var exception = Record.Exception(debugger.Stop);

        Assert.Null(exception);
    }

    [Fact]
    public async Task Run_WithoutSettingsService_Throws()
    {
        using var debugger = CreateDebugger();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => debugger.Run());

        Assert.Contains(nameof(ConfigDebugger.RuriLibSettings), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_WithoutRngProvider_Throws()
    {
        using var debugger = CreateDebugger();
        debugger.RuriLibSettings = CreateSettingsService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => debugger.Run());

        Assert.Contains(nameof(ConfigDebugger.RNGProvider), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_WithoutPluginRepository_Throws()
    {
        using var debugger = CreateDebugger();
        debugger.RuriLibSettings = CreateSettingsService();
        debugger.RNGProvider = new global::RuriLib.Providers.RandomNumbers.DefaultRNGProvider();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => debugger.Run());

        Assert.Contains(nameof(ConfigDebugger.PluginRepo), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_WithInvalidProxy_ResetsStatusToIdle()
    {
        using var debugger = CreateDebugger(new DebuggerOptions
        {
            TestData = "test",
            WordlistType = "Default",
            UseProxy = true,
            TestProxy = "http://test:8000:user:pass"
        });

        debugger.RuriLibSettings = CreateSettingsService();
        debugger.RNGProvider = new global::RuriLib.Providers.RandomNumbers.DefaultRNGProvider();
        debugger.PluginRepo = CreatePluginRepository();

        await Assert.ThrowsAsync<FormatException>(() => debugger.Run());

        Assert.Equal(ConfigDebuggerStatus.Idle, debugger.Status);
    }

    [Fact]
    public async Task Run_WhenTestDataDoesNotMatchWordlistRegex_LogsWarning()
    {
        using var debugger = CreateDebugger(new Config
        {
            Id = "wordlist-warning-test",
            Mode = ConfigMode.Legacy,
            LoliScript = "PRINT \"done\""
        }, new DebuggerOptions
        {
            TestData = "abc",
            WordlistType = "StrictNumeric"
        });

        debugger.RuriLibSettings = CreateSettingsService(settings =>
        {
            settings.Environment.WordlistTypes.Add(new WordlistType
            {
                Name = "StrictNumeric",
                Regex = "^[0-9]+$",
                Verify = true,
                Separator = string.Empty,
                Slices = ["DATA"]
            });
        });
        debugger.RNGProvider = new global::RuriLib.Providers.RandomNumbers.DefaultRNGProvider();
        debugger.PluginRepo = CreatePluginRepository();

        await debugger.Run();

        Assert.Contains(debugger.Logger.Entries, e => e.Message.Contains(
            "WARNING: The test input data did not respect the validity regex for the selected wordlist type!",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task Run_WhenTestDataDoesNotMatchDataRules_LogsWarning()
    {
        using var debugger = CreateDebugger(new Config
        {
            Id = "data-rules-warning-test",
            Mode = ConfigMode.Legacy,
            LoliScript = "PRINT \"done\"",
            Settings =
            {
                DataSettings =
                {
                    DataRules =
                    [
                        new SimpleDataRule
                        {
                            SliceName = "DATA",
                            Comparison = StringRule.EqualTo,
                            StringToCompare = "expected",
                            CaseSensitive = true
                        }
                    ]
                }
            }
        }, new DebuggerOptions
        {
            TestData = "actual",
            WordlistType = "Default"
        });

        debugger.RuriLibSettings = CreateSettingsService();
        debugger.RNGProvider = new global::RuriLib.Providers.RandomNumbers.DefaultRNGProvider();
        debugger.PluginRepo = CreatePluginRepository();

        await debugger.Run();

        Assert.Contains(debugger.Logger.Entries, e => e.Message.Contains(
            "WARNING: The test input data did not respect the data rules of this config!",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task Run_LegacyConfigInStepByStepMode_WaitsForExplicitStep()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var debugger = CreateDebugger(new Config
        {
            Id = "legacy-test",
            Mode = ConfigMode.Legacy,
            LoliScript = "PRINT \"first\"\nPRINT \"second\""
        }, new DebuggerOptions
        {
            TestData = "test",
            WordlistType = "Default",
            StepByStep = true
        });

        debugger.RuriLibSettings = CreateSettingsService();
        debugger.RNGProvider = new global::RuriLib.Providers.RandomNumbers.DefaultRNGProvider();
        debugger.PluginRepo = CreatePluginRepository();

        var waitingForStep = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        debugger.StatusChanged += (_, status) =>
        {
            if (status == ConfigDebuggerStatus.WaitingForStep)
            {
                waitingForStep.TrySetResult();
            }
        };

        var runTask = debugger.Run();

        await waitingForStep.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Assert.Equal(ConfigDebuggerStatus.WaitingForStep, debugger.Status);
        Assert.Contains(debugger.Logger.Entries, e => e.Message == "\"first\"");
        Assert.DoesNotContain(debugger.Logger.Entries, e => e.Message == "\"second\"");
        Assert.True(debugger.TryTakeStep());

        await runTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Assert.Equal(ConfigDebuggerStatus.Idle, debugger.Status);
        Assert.Contains(debugger.Logger.Entries, e => e.Message == "\"second\"");
    }

    [Fact]
    public async Task Run_LoliCodeConfigInStepByStepMode_UpdatesVariablesBeforeNextStep()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var debugger = CreateDebugger(new Config
        {
            Id = "lolicode-test",
            Mode = ConfigMode.LoliCode,
            LoliCodeScript = """
                SET VAR myVar "first"
                BLOCK:ConstantString
                value = "second"
                => VAR @secondVar
                ENDBLOCK
                """
        }, new DebuggerOptions
        {
            TestData = "test",
            WordlistType = "Default",
            StepByStep = true
        });

        debugger.RuriLibSettings = CreateSettingsService();
        debugger.RNGProvider = new global::RuriLib.Providers.RandomNumbers.DefaultRNGProvider();
        debugger.PluginRepo = CreatePluginRepository();

        var waitingForStep = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        debugger.StatusChanged += (_, status) =>
        {
            if (status == ConfigDebuggerStatus.WaitingForStep)
            {
                waitingForStep.TrySetResult();
            }
        };

        var runTask = debugger.Run();

        await waitingForStep.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Assert.Equal(ConfigDebuggerStatus.WaitingForStep, debugger.Status);
        Assert.Contains(debugger.Options.Variables, v => v.Name == "myVar" && v.AsString() == "first");
        Assert.DoesNotContain(debugger.Options.Variables, v => v.Name == "secondVar");
        Assert.True(debugger.TryTakeStep());

        await runTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Assert.Equal(ConfigDebuggerStatus.Idle, debugger.Status);
        Assert.Contains(debugger.Options.Variables, v => v.Name == "secondVar" && v.AsString() == "second");
    }

    [Fact]
    public async Task Run_LoliCodeConfigInStepByStepMode_DoesNotLoseScopedVariablesAfterConditionalBlock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var debugger = CreateDebugger(new Config
        {
            Id = "lolicode-conditional-scope-test",
            Mode = ConfigMode.LoliCode,
            LoliCodeScript = """
                IF true
                BLOCK:ConstantString
                value = "inner"
                => VAR @token
                ENDBLOCK
                END
                BLOCK:ConstantString
                value = "outer"
                => VAR @result
                ENDBLOCK
                """
        }, new DebuggerOptions
        {
            TestData = "test",
            WordlistType = "Default",
            StepByStep = true
        });

        debugger.RuriLibSettings = CreateSettingsService();
        debugger.RNGProvider = new global::RuriLib.Providers.RandomNumbers.DefaultRNGProvider();
        debugger.PluginRepo = CreatePluginRepository();

        var waitingCount = 0;
        debugger.StatusChanged += (_, status) =>
        {
            if (status == ConfigDebuggerStatus.WaitingForStep)
            {
                Interlocked.Increment(ref waitingCount);
            }
        };

        var runTask = debugger.Run();
        await WaitForStepCountAsync(expectedCount: 1, () => waitingCount, cancellationToken);
        Assert.Equal(ConfigDebuggerStatus.WaitingForStep, debugger.Status);
        Assert.Empty(debugger.Options.Variables);
        Assert.True(debugger.TryTakeStep());

        await WaitForStepCountAsync(expectedCount: 2, () => waitingCount, cancellationToken);
        Assert.Equal(ConfigDebuggerStatus.WaitingForStep, debugger.Status);
        Assert.Contains(debugger.Options.Variables, v => v.Name == "token" && v.AsString() == "inner");
        Assert.DoesNotContain(debugger.Options.Variables, v => v.Name == "result");
        Assert.True(debugger.TryTakeStep());

        await WaitForStepCountAsync(expectedCount: 3, () => waitingCount, cancellationToken);
        Assert.Equal(ConfigDebuggerStatus.WaitingForStep, debugger.Status);
        Assert.Contains(debugger.Options.Variables, v => v.Name == "token" && v.AsString() == "inner");
        Assert.DoesNotContain(debugger.Options.Variables, v => v.Name == "result");
        Assert.True(debugger.TryTakeStep());

        await runTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Assert.Equal(ConfigDebuggerStatus.Idle, debugger.Status);
        Assert.Contains(debugger.Options.Variables, v => v.Name == "token" && v.AsString() == "inner");
        Assert.Contains(debugger.Options.Variables, v => v.Name == "result" && v.AsString() == "outer");
    }

    private static async Task WaitForStepCountAsync(
        int expectedCount,
        Func<int> getCurrentCount,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        while (getCurrentCount() < expectedCount)
        {
            await Task.Delay(50, cts.Token);
        }
    }

    private static ConfigDebugger CreateDebugger(DebuggerOptions? options = null)
        => CreateDebugger(new Config { Id = "test" }, options);

    private static ConfigDebugger CreateDebugger(Config config, DebuggerOptions? options = null)
        => new(config, options ?? new DebuggerOptions(), new BotLogger());

    private static global::RuriLib.Services.RuriLibSettingsService CreateSettingsService(
        Action<global::RuriLib.Services.RuriLibSettingsService>? configure = null)
    {
        var settings = new global::RuriLib.Services.RuriLibSettingsService(
            Path.Combine(Path.GetTempPath(), $"ob2-config-debugger-tests-{Guid.NewGuid():N}"));

        configure?.Invoke(settings);
        return settings;
    }

    private static global::RuriLib.Services.PluginRepository CreatePluginRepository()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-plugin-repo-tests-{Guid.NewGuid():N}"));
}
