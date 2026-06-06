using Microsoft.Extensions.Logging;
using OpenBullet2.Core.Services;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using RuriLib.Services;
using System.Collections.Generic;
using MonitorAction = RuriLib.Models.Jobs.Monitor.Actions.Action;

namespace OpenBullet2.Core.Tests.Services;

public class TriggeredActionExecutorTests
{
    private static readonly string TestRoot = Path.Combine(Path.GetTempPath(), "OpenBullet2", nameof(TriggeredActionExecutorTests));
    private static readonly RuriLibSettingsService SettingsService = new(Path.Combine(TestRoot, "Settings"));
    private static readonly PluginRepository PluginRepository = new(Path.Combine(TestRoot, "Plugins"));

    [Fact]
    public async Task CheckAndExecuteAsync_WhenTriggersMatch_ExecutesActionsAndLogsInformation()
    {
        var logger = new TestLogger<TriggeredActionExecutor>();
        var executor = new TriggeredActionExecutor(logger);
        var trackedAction = new TrackingAction();
        var monitoredJob = new FakeJob { Id = 12, Name = "Monitored job" };
        var triggeredAction = new TriggeredAction
        {
            Name = "On Idle",
            JobId = monitoredJob.Id,
            Triggers = [new JobStatusTrigger { Status = JobStatus.Idle }],
            Actions = [trackedAction]
        };

        await executor.CheckAndExecuteAsync(triggeredAction, [monitoredJob]);

        Assert.True(trackedAction.Executed);
        Assert.Equal(1, triggeredAction.Executions);
        Assert.False(triggeredAction.IsExecuting);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information &&
            e.Message.Contains("Triggered action", StringComparison.Ordinal));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information &&
            e.Message.Contains(nameof(TrackingAction), StringComparison.Ordinal));
    }

    [Fact]
    public async Task CheckAndExecuteAsync_WhenAnActionThrows_ContinuesWithTheNextAction()
    {
        var logger = new TestLogger<TriggeredActionExecutor>();
        var executor = new TriggeredActionExecutor(logger);
        var trackedAction = new TrackingAction();
        var monitoredJob = new FakeJob { Id = 18, Name = "Monitored job" };
        var triggeredAction = new TriggeredAction
        {
            Name = "Keep going",
            JobId = monitoredJob.Id,
            Triggers = [new JobStatusTrigger { Status = JobStatus.Idle }],
            Actions = [new ThrowingAction(), trackedAction]
        };

        await executor.CheckAndExecuteAsync(triggeredAction, [monitoredJob]);

        Assert.True(trackedAction.Executed);
        Assert.Equal(1, triggeredAction.Executions);
        Assert.False(triggeredAction.IsExecuting);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning &&
            e.Message.Contains(nameof(ThrowingAction), StringComparison.Ordinal));
    }

    [Fact]
    public async Task CheckAndExecuteAsync_WhenATriggerThrows_DoesNotExecuteActionsAndLogsWarning()
    {
        var logger = new TestLogger<TriggeredActionExecutor>();
        var executor = new TriggeredActionExecutor(logger);
        var trackedAction = new TrackingAction();
        var monitoredJob = new FakeJob { Id = 42, Name = "Monitored job" };
        var triggeredAction = new TriggeredAction
        {
            Name = "Broken trigger",
            JobId = monitoredJob.Id,
            Triggers = [new ThrowingTrigger()],
            Actions = [trackedAction]
        };

        await executor.CheckAndExecuteAsync(triggeredAction, [monitoredJob]);

        Assert.False(trackedAction.Executed);
        Assert.Equal(0, triggeredAction.Executions);
        Assert.False(triggeredAction.IsExecuting);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning &&
            e.Message.Contains("Failed to evaluate triggers", StringComparison.Ordinal));
    }

    private sealed class FakeJob : Job
    {
        public FakeJob() : base(SettingsService, PluginRepository)
        {
        }

        public override TimeSpan Remaining => TimeSpan.Zero;
        public override float Progress => 0f;
        public override Task Pause() => Task.CompletedTask;
        public override Task Resume() => Task.CompletedTask;
        public override Task Stop() => Task.CompletedTask;
        public override Task Abort() => Task.CompletedTask;
    }

    private sealed class TrackingAction : MonitorAction
    {
        public bool Executed { get; private set; }

        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
        {
            Executed = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingAction : MonitorAction
    {
        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
            => throw new InvalidOperationException("Action failure");
    }

    private sealed class ThrowingTrigger : Trigger
    {
        public override bool CheckStatus(Job job)
            => throw new InvalidOperationException("Trigger failure");
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose()
        {
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
}
