using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RuriLib.Logging
{
    /// <summary>
    /// Stores logs for operations that a bot executes.
    /// </summary>
    public interface IBotLogger
    {
        /// <summary>
        /// Whether the log is enabled. If disabled, calling methods that would add entries
        /// to the log will not add anything.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// A copy of the list of entries in the log. Since it's a copy, it's
        /// safe to enumerate as it will not change during enumeration.
        /// </summary>
        IEnumerable<BotLoggerEntry> Entries { get; }

        /// <summary>
        /// The name of the block that is currently being executed.
        /// </summary>
        string ExecutingBlock { get; set; }

        /// <summary>
        /// Called when a new entry was written to the log.
        /// </summary>
        event EventHandler<BotLoggerEntry> NewEntry;

        /// <summary>
        /// Logs the name of the method that called this method.
        /// </summary>
        void LogHeader([CallerMemberName] string caller = null);

        /// <summary>
        /// Logs a new <paramref name="message"/> with a given <paramref name="color"/>.
        /// If the <paramref name="message"/> contains HTML code, set <paramref name="canViewAsHtml"/> to true.
        /// </summary>
        void Log(string message, string color = "#fff", bool canViewAsHtml = false);

        /// <summary>
        /// Logs a multi-line message (with lines stored in an <paramref name="enumerable"/>) with a given <paramref name="color"/>.
        /// If the <paramref name="message"/> contains HTML code, set <paramref name="canViewAsHtml"/> to true.
        /// </summary>
        void Log(IEnumerable<string> enumerable, string color = "#fff", bool canViewAsHtml = false);

        /// <summary>
        /// Clears all entries of the log.
        /// </summary>
        void Clear();

        /// <summary>
        /// Logs the string representation of an <paramref name="obj"/> with a given <paramref name="color"/>.
        /// If the <paramref name="obj"/> contains HTML code, set <paramref name="canViewAsHtml"/> to true.
        /// </summary>
        void LogObject(object obj, string color = "#fff", bool canViewAsHtml = false);
    }
}
