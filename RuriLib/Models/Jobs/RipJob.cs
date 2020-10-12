using RuriLib.Services;

namespace RuriLib.Models.Jobs
{
    public class RipJob : Job
    {
        public RipJob(RuriLibSettingsService settings) : base(settings)
        {
        }
    }
}
