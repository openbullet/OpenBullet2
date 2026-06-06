using RuriLib.Logging;
using RuriLib.Services;

namespace RuriLib.Models.Jobs;

/// <summary>
/// Represents a ripping job.
/// </summary>
public class RipJob : Job
{
    /// <summary>
    /// Creates a ripping job.
    /// </summary>
    /// <param name="settings">The RuriLib settings service.</param>
    /// <param name="pluginRepo">The plugin repository.</param>
    /// <param name="logger">The optional job logger.</param>
    public RipJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger? logger = null)
        : base(settings, pluginRepo, logger)
    {
    }
}
