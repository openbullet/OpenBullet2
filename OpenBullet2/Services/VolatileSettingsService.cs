using OpenBullet2.Models.Debugger;
using RuriLib.Logging;

namespace OpenBullet2.Services
{
    public class VolatileSettingsService
    {
        public DebuggerOptions DebuggerOptions { get; set; }
        public BotLogger DebuggerLog { get; set; }

        public VolatileSettingsService(PersistentSettingsService persistentSettings)
        {
            DebuggerOptions = new DebuggerOptions(persistentSettings);
            DebuggerLog = new BotLogger();
        }
    }
}
