using RuriLib.Logging;
using RuriLib.Services;

namespace RuriLib.Models.Jobs
{
    public class RipJob : Job
    {
        public RipJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
            : base(settings, pluginRepo, logger)
        {
        }
    }
}
