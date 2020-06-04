using RuriLib.Models.Proxies;
using RuriLib.Services;
using System.Linq;

namespace OpenBullet2.Models.Debugger
{
    public class DebuggerOptions
    {
        public string TestData { get; set; } = "";
        public string WordlistType { get; set; }
        public string TestProxy { get; set; } = "";
        public bool UseProxy { get; set; } = false;
        public ProxyType ProxyType { get; set; } = ProxyType.Http;
        public bool PersistLog { get; set; } = false;

        public DebuggerOptions(RuriLibSettingsService settings)
        {
            WordlistType = settings.Environment.WordlistTypes.First().Name;
        }
    }
}
