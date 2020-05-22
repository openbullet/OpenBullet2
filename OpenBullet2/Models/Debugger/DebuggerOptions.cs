using OpenBullet2.Services;
using RuriLib.Models.Proxies;
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

        public DebuggerOptions(PersistentSettingsService settings)
        {
            WordlistType = settings.Environment.WordlistTypes.First().Name;
        }
    }
}
