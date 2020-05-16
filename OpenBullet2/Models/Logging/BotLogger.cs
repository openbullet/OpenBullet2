using RuriLib.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OpenBullet2.Models.Logging
{
    public class BotLogger : IBotLogger
    {
        public List<BotLogEntry> Entries { get; set; } = new List<BotLogEntry>();

        public void Log(string message, string color = "#fff")
        {
            Entries.Add(new BotLogEntry 
            {
                Message = message,
                Color = color
            });
        }

        public void Log(IEnumerable<string> enumerable, string color = "#fff")
        {
            Entries.Add(new BotLogEntry 
            {
                Message = string.Join(Environment.NewLine, enumerable),
                Color = color
            });
        }

        public void LogHeader([CallerMemberName] string caller = null)
        {
            Entries.Add(new BotLogEntry
            {
                Message = $">> {caller} <<",
                Color = LogColors.ChromeYellow
            });
        }

        public void Clear()
        {
            Entries.Clear();
        }
    }
}
