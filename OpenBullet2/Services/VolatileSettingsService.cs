using GridShared.Utility;
using Microsoft.Extensions.Primitives;
using RuriLib.Logging;
using RuriLib.Models.Blocks;
using RuriLib.Models.Debugger;
using RuriLib.Services;
using System.Collections.Generic;
using System.Linq;

namespace OpenBullet2.Services
{
    public class VolatileSettingsService
    {
        public DebuggerOptions DebuggerOptions { get; set; }
        public BotLogger DebuggerLog { get; set; }
        public List<BlockDescriptor> RecentDescriptors { get; set; }
        public bool ConfigsDetailedView { get; set; } = false;
        public Dictionary<(int, string), QueryDictionary<StringValues>> GridQueries { get; set; } = new();

        public VolatileSettingsService(RuriLibSettingsService ruriLibSettings)
        {
            DebuggerOptions = new DebuggerOptions
            {
                WordlistType = ruriLibSettings.Environment.WordlistTypes.First().Name 
            };
            DebuggerLog = new();
            RecentDescriptors = new();
        }

        public void AddRecentDescriptor(BlockDescriptor descriptor)
        {
            if (RecentDescriptors.Contains(descriptor))
            {
                RecentDescriptors.Remove(descriptor);
            }

            RecentDescriptors.Insert(0, descriptor);
        }
    }
}
