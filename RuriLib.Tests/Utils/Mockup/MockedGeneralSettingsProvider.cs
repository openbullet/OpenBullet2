using RuriLib.Providers.Proxies;

namespace RuriLib.Tests.Utils.Mockup
{
    public class MockedGeneralSettingsProvider : IGeneralSettingsProvider
    {
        public bool VerboseMode => true;
        public bool LogAllResults => true;
    }
}
