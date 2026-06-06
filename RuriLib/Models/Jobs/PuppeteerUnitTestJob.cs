using RuriLib.Logging;
using RuriLib.Services;

namespace RuriLib.Models.Jobs;

/// <summary>
/// Represents a job that runs Puppeteer unit tests.
/// </summary>
public class PuppeteerUnitTestJob : Job
{
    /// <summary>
    /// Creates a Puppeteer unit-test job.
    /// </summary>
    /// <param name="settings">The RuriLib settings service.</param>
    /// <param name="pluginRepo">The plugin repository.</param>
    /// <param name="logger">The optional job logger.</param>
    public PuppeteerUnitTestJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger? logger = null)
        : base(settings, pluginRepo, logger)
    {
    }
}
