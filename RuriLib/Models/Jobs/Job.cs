using RuriLib.Logging;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs;

/// <summary>
/// Base class for executable jobs.
/// </summary>
public abstract class Job : IDisposable
{
    // Public properties
    /// <summary>Gets or sets the job identifier.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the job name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the owner identifier.</summary>
    public int OwnerId { get; set; }
    /// <summary>Gets the current job status.</summary>
    public JobStatus Status { get; protected set; } = JobStatus.Idle;
    /// <summary>Gets the outcome of the most recent run.</summary>
    public JobLastRunOutcome LastRunOutcome { get; protected set; } = JobLastRunOutcome.None;
    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;
    /// <summary>Gets or sets the start timestamp.</summary>
    public DateTime StartTime { get; set; } = DateTime.Now;
    /// <summary>Gets or sets the condition that controls when the job may start.</summary>
    public StartCondition StartCondition { get; set; } = new RelativeTimeStartCondition();
    /// <summary>Gets the elapsed runtime.</summary>
    public virtual TimeSpan Elapsed => DateTime.Now - StartTime;
    /// <summary>Gets the remaining runtime estimate.</summary>
    public virtual TimeSpan Remaining => throw new NotImplementedException();

    // Virtual properties
    /// <summary>Gets the current progress percentage.</summary>
    public virtual float Progress => throw new NotImplementedException();

    // Protected fields
    /// <summary>The settings service used by the job.</summary>
    protected readonly RuriLibSettingsService settings;
    /// <summary>The plugin repository used by the job.</summary>
    protected readonly PluginRepository pluginRepo;
    /// <summary>The optional logger used by the job.</summary>
    protected readonly IJobLogger? logger;

    // Private fields
    private bool disposed;
    private bool waitFinished;
    private CancellationTokenSource? cts; // Cancellation token for cancelling the StartCondition wait

    /// <summary>
    /// Creates a job.
    /// </summary>
    /// <param name="settings">The RuriLib settings service.</param>
    /// <param name="pluginRepo">The plugin repository.</param>
    /// <param name="logger">The optional job logger.</param>
    public Job(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger? logger = null)
    {
        this.settings = settings;
        this.pluginRepo = pluginRepo;
        this.logger = logger;
    }

    /// <summary>
    /// Starts the job after waiting for its start condition.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when startup has finished.</returns>
    public virtual async Task Start(CancellationToken cancellationToken = default)
    {
        waitFinished = false;
        cts?.Dispose();
        cts = new CancellationTokenSource();

        StartTime = DateTime.Now;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        try
        {
            logger?.LogInfo(Id, "Waiting for the start condition to be verified...");
            await StartCondition.WaitUntilVerified(this, linkedCts.Token);
            logger?.LogInfo(Id, "Finished waiting");
        }
        catch (TaskCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // The token has been cancelled, skip the wait
            logger?.LogInfo(Id, "The wait has been manually skipped");
        }

        waitFinished = true;
    }

    /// <summary>
    /// Skips the current start-condition wait.
    /// </summary>
    public void SkipWait()
    {
        if (!waitFinished && cts is not null && !cts.IsCancellationRequested)
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Pauses the job.
    /// </summary>
    /// <returns>A task that completes when the job is paused.</returns>
    public virtual Task Pause()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Resumes the job.
    /// </summary>
    /// <returns>A task that completes when the job is resumed.</returns>
    public virtual Task Resume()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Stops the job gracefully.
    /// </summary>
    /// <returns>A task that completes when the job has stopped.</returns>
    public virtual Task Stop()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Aborts the job immediately.
    /// </summary>
    /// <returns>A task that completes when the job has been aborted.</returns>
    public virtual Task Abort()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Restores the outcome of the most recent run from persisted state.
    /// </summary>
    /// <param name="lastRunOutcome">The outcome to restore.</param>
    public void RestoreLastRunOutcome(JobLastRunOutcome lastRunOutcome)
    {
        LastRunOutcome = lastRunOutcome;
    }

    /// <summary>
    /// Disposes the job resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the job resources.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed || !disposing)
        {
            return;
        }

        if (!waitFinished && cts is not null && !cts.IsCancellationRequested)
        {
            try
            {
                cts.Cancel();
            }
            catch
            {
                // ignored
            }
        }

        cts?.Dispose();
        cts = null;
        disposed = true;
    }
}
