using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RuriLib.Logging
{
    public interface IBotLogger
    {
        bool Enabled { get; set; }
        IEnumerable<BotLoggerEntry> Entries { get; }
        string ExecutingBlock { get; set; }
        event EventHandler<BotLoggerEntry> NewEntry;
        void LogHeader([CallerMemberName] string caller = null);
        void Log(string message, string color = "#fff", bool canViewAsHtml = false);
        void Log(IEnumerable<string> enumerable, string color = "#fff", bool canViewAsHtml = false);
        void Clear();
    }
}
