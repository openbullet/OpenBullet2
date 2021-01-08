using RuriLib.Logging;
using RuriLib.Services;

namespace RuriLib.Models.Jobs
{
    public class SpiderJob : Job
    {
        public SpiderJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
            : base(settings, pluginRepo, logger)
        {
        }
    }
}
