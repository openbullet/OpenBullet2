using RuriLib.Logging;
using RuriLib.Services;

namespace RuriLib.Models.Jobs
{
    public class SeleniumUnitTestJob : Job
    {
        public SeleniumUnitTestJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
            : base(settings, pluginRepo, logger)
        {
        }
    }
}
