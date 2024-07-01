using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;

namespace OpenBullet2.Core.Models.Jobs;

/// <summary>
/// Base class for options of a <see cref="Job"/>.
/// </summary>
public abstract class JobOptions
{
    /// <summary>
    /// The condition that needs to be verified in order to start the job.
    /// </summary>
    public StartCondition StartCondition { get; set; } = new RelativeTimeStartCondition();

    /// <summary>
    /// The name of the job.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
