using RuriLib.Services;

namespace RuriLib.Models.Jobs
{
    public class SeleniumUnitTestJob : Job
    {
        public SeleniumUnitTestJob(RuriLibSettingsService settings) : base(settings)
        {
        }
    }
}
