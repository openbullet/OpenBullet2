using Microsoft.Extensions.Logging;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitorAction = RuriLib.Models.Jobs.Monitor.Actions.Action;

namespace OpenBullet2.Core.Services;

/// <summary>
/// Evaluates monitor triggers and executes the configured actions.
/// </summary>
public class TriggeredActionExecutor(ILogger<TriggeredActionExecutor> logger)
{
    private readonly ILogger<TriggeredActionExecutor> _logger = logger;

    /// <summary>
    /// Checks the triggers and executes the actions when they all match.
    /// </summary>
    /// <param name="triggeredAction">The triggered action to evaluate.</param>
    /// <param name="jobs">The available jobs.</param>
    /// <returns>A task that completes when the evaluation finishes.</returns>
    public async Task CheckAndExecuteAsync(TriggeredAction triggeredAction, IEnumerable<Job> jobs)
    {
        var jobsArray = jobs as Job[] ?? jobs.ToArray();
        var job = jobsArray.FirstOrDefault(j => j.Id == triggeredAction.JobId);

        if (job is null)
        {
            _logger.LogDebug(
                "Skipping triggered action {TriggeredActionId} ({TriggeredActionName}) because job {JobId} was not found",
                triggeredAction.Id,
                triggeredAction.Name,
                triggeredAction.JobId);

            return;
        }

        try
        {
            if (!triggeredAction.Triggers.All(t => t.CheckStatus(job)))
            {
                return;
            }

            triggeredAction.Executions++;
            triggeredAction.IsExecuting = true;

            _logger.LogInformation(
                "Triggered action {TriggeredActionId} ({TriggeredActionName}) fired for job {JobId} ({JobName})",
                triggeredAction.Id,
                triggeredAction.Name,
                job.Id,
                job.Name);

            foreach (var action in triggeredAction.Actions)
            {
                try
                {
                    await action.Execute(triggeredAction.JobId, jobsArray);

                    _logger.LogInformation(
                        "Executed monitor action {MonitorActionType} for triggered action {TriggeredActionId} ({TriggeredActionName})",
                        GetActionName(action),
                        triggeredAction.Id,
                        triggeredAction.Name);
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to execute monitor action {MonitorActionType} for triggered action {TriggeredActionId} ({TriggeredActionName})",
                        GetActionName(action),
                        triggeredAction.Id,
                        triggeredAction.Name);
                }
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to evaluate triggers for triggered action {TriggeredActionId} ({TriggeredActionName}) on job {JobId} ({JobName})",
                triggeredAction.Id,
                triggeredAction.Name,
                job.Id,
                job.Name);
        }
        finally
        {
            triggeredAction.IsExecuting = false;
        }
    }

    private static string GetActionName(MonitorAction action) => action.GetType().Name;
}
