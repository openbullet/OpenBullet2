using RuriLib.Logging;
using RuriLib.Services;

namespace RuriLib.Models.Jobs;

/// <summary>
/// Represents a spidering job.
/// </summary>
public class SpiderJob : Job
{
    /// <summary>
    /// Creates a spidering job.
    /// </summary>
    /// <param name="settings">The RuriLib settings service.</param>
    /// <param name="pluginRepo">The plugin repository.</param>
    /// <param name="logger">The optional job logger.</param>
    public SpiderJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger? logger = null)
        : base(settings, pluginRepo, logger)
    {
    }
}
