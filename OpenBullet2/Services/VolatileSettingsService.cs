using OpenBullet2.Models.Debugger;
using RuriLib.Logging;
using RuriLib.Services;

namespace OpenBullet2.Services
{
    public class VolatileSettingsService
    {
        public DebuggerOptions DebuggerOptions { get; set; }
        public BotLogger DebuggerLog { get; set; }

        public VolatileSettingsService(RuriLibSettingsService ruriLibSettings)
        {
            DebuggerOptions = new DebuggerOptions(ruriLibSettings);
            DebuggerLog = new BotLogger();
        }
    }
}
