using RuriLib.Services;

namespace RuriLib.Models.Jobs
{
    public class SpiderJob : Job
    {
        public SpiderJob(RuriLibSettingsService settings) : base(settings)
        {
        }
    }
}
