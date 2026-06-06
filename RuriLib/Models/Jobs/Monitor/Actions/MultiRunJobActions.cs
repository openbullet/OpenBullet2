using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.Monitor.Actions;

/// <summary>
/// Base class for actions that target a <see cref="MultiRunJob"/>.
/// </summary>
public class MultiRunJobAction : Action
{
    /// <summary>Gets or sets the target job identifier.</summary>
    public int TargetJobId { get; set; }

    /// <inheritdoc />
    public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
    {
        // Older monitor actions, including ones created through the web UI,
        // do not persist an explicit target job id. In that case the action
        // must operate on the monitored job itself.
        var targetJobId = TargetJobId > 0 ? TargetJobId : currentJobId;
        var job = jobs.FirstOrDefault(j => j.Id == targetJobId) as MultiRunJob;

        if (job is null)
        {
            throw new InvalidOperationException($"The job with id {targetJobId} is not a MultiRunJob");
        }

        return Execute(job);
    }

    /// <summary>
    /// Executes the action against a <see cref="MultiRunJob"/>.
    /// </summary>
    /// <param name="job">The target job.</param>
    /// <returns>A task that completes when the action finishes.</returns>
    public virtual Task Execute(MultiRunJob job)
        => throw new NotImplementedException();
}

/// <summary>
/// Changes the bot count of a target multi-run job.
/// </summary>
public class SetBotsAction : MultiRunJobAction
{
    /// <summary>Gets or sets the new bot count.</summary>
    public int Amount { get; set; }

    /// <inheritdoc />
    public override Task Execute(MultiRunJob job)
    {
        if (Amount is > 0 and <= 200)
        {
            job.Bots = Amount;
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Changes the skip value of a target multi-run job.
/// The new value affects the next start of the job.
/// </summary>
public class SetSkipAction : MultiRunJobAction
{
    /// <summary>Gets or sets the new skip value.</summary>
    public int Skip { get; set; }

    /// <inheritdoc />
    public override Task Execute(MultiRunJob job)
    {
        if (Skip < 0)
        {
            return Task.CompletedTask;
        }

        var dataPoolSize = job.DataPool?.Size;

        if (dataPoolSize is null or <= 0 || Skip < dataPoolSize)
        {
            job.Skip = Skip;
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Reloads the proxies of a target multi-run job.
/// </summary>
public class ReloadProxiesAction : MultiRunJobAction
{
    /// <inheritdoc />
    public override Task Execute(MultiRunJob job) => job.FetchProxiesFromSources();
}
