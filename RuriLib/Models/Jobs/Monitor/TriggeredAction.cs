using RuriLib.Models.Jobs.Monitor.Triggers;
using System;
using System.Collections.Generic;
using Action = RuriLib.Models.Jobs.Monitor.Actions.Action;

namespace RuriLib.Models.Jobs.Monitor;

/// <summary>
/// Combines triggers and actions that run against a monitored job.
/// </summary>
public class TriggeredAction
{
    /// <summary>Gets the unique identifier.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();
    /// <summary>Gets the display name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets or sets a value indicating whether the triggered action is active.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether the triggered action is currently executing.</summary>
    public bool IsExecuting { get; set; }
    /// <summary>Gets or sets a value indicating whether the triggered action can execute multiple times.</summary>
    public bool IsRepeatable { get; set; }
    /// <summary>Gets or sets the number of executions.</summary>
    public int Executions { get; set; }

    // The job this triggered action refers to
    /// <summary>Gets or sets the identifier of the monitored job.</summary>
    public int JobId { get; set; }

    // All triggers must be verified at the same time
    /// <summary>Gets the trigger list.</summary>
    public List<Trigger> Triggers { get; init; } = [];

    // Actions are executed sequentially, so stop - delay - start is possible
    /// <summary>Gets the action list.</summary>
    public List<Action> Actions { get; init; } = [];

    /// <summary>
    /// Resets the execution state and counter.
    /// </summary>
    public void Reset()
    {
        IsExecuting = false;
        Executions = 0;
    }
}
