using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using RuriLib.Services;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Jobs.Monitor;

public class MonitorModelsTests
{
    [Fact]
    public void TriggeredAction_WhenReset_ClearsExecutionState()
    {
        var action = new TriggeredAction
        {
            JobId = 123,
            IsExecuting = true,
            Executions = 3,
            Triggers = [],
            Actions = []
        };

        action.Reset();

        Assert.False(action.IsExecuting);
        Assert.Equal(0, action.Executions);
    }

    [Fact]
    public async Task MultiRunJobAction_WhenTargetIsNotMultiRunJob_ThrowsInvalidOperationException()
    {
        var action = new SetBotsAction
        {
            TargetJobId = 1,
            Amount = 5
        };
        var jobs = new Job[] { new TestJob(CreateSettingsService(), CreatePluginRepository()) { Id = 1 } };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => action.Execute(1, jobs));

        Assert.Equal("The job with id 1 is not a MultiRunJob", exception.Message);
    }

    [Fact]
    public void MultiRunJobTrigger_WhenJobIsNotMultiRunJob_ThrowsInvalidOperationException()
    {
        var trigger = new TestedCountTrigger
        {
            Comparison = NumComparison.GreaterThanOrEqualTo,
            Amount = 0
        };
        var job = new TestJob(CreateSettingsService(), CreatePluginRepository());

        var exception = Assert.Throws<InvalidOperationException>(() => trigger.CheckStatus(job));

        Assert.Equal("The job must be a MultiRunJob", exception.Message);
    }

    [Fact]
    public void TriggeredAction_WhenConfigured_StoresTriggersAndActions()
    {
        var action = new TriggeredAction
        {
            JobId = 7,
            Triggers = [new JobStatusTrigger { Status = JobStatus.Running }],
            Actions = [new SetBotsAction { TargetJobId = 7, Amount = 4 }]
        };

        Assert.Single(action.Triggers);
        Assert.Single(action.Actions);
        Assert.Equal(7, action.JobId);
    }

    [Fact]
    public async Task MultiRunJobAction_WhenTargetJobIdIsMissing_UsesCurrentJobId()
    {
        var action = new SetBotsAction
        {
            Amount = 7
        };
        var job = new TestMultiRunJob(CreateSettingsService(), CreatePluginRepository())
        {
            Id = 3,
            Bots = 1
        };

        await action.Execute(job.Id, [job]);

        Assert.Equal(7, job.Bots);
    }

    [Fact]
    public async Task SetSkipAction_WhenTargetJobIdIsMissing_UsesCurrentJobId()
    {
        var action = new SetSkipAction
        {
            Skip = 12
        };
        var job = new TestMultiRunJob(CreateSettingsService(), CreatePluginRepository())
        {
            Id = 3,
            Skip = 1
        };

        await action.Execute(job.Id, [job]);

        Assert.Equal(12, job.Skip);
    }

    private static RuriLibSettingsService CreateSettingsService()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-monitor-tests-{Guid.NewGuid():N}"));

    private static PluginRepository CreatePluginRepository()
        => (PluginRepository)RuntimeHelpers.GetUninitializedObject(typeof(PluginRepository));

    private sealed class TestJob(RuriLibSettingsService settings, PluginRepository pluginRepo) : Job(settings, pluginRepo)
    {
    }

    private sealed class TestMultiRunJob(RuriLibSettingsService settings, PluginRepository pluginRepo)
        : MultiRunJob(settings, pluginRepo)
    {
        public new int Bots
        {
            get => base.Bots;
            set => base.Bots = value;
        }
    }

}
