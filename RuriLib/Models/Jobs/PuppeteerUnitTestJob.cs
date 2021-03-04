using RuriLib.Logging;
using RuriLib.Services;

namespace RuriLib.Models.Jobs
{
    public class PuppeteerUnitTestJob : Job
    {
        public PuppeteerUnitTestJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
            : base(settings, pluginRepo, logger)
        {
        }
    }
}
