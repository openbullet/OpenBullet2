using RuriLib.Attributes;
using RuriLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RuriLib.Logging
{
    /// An <see cref="IBotLogger"/> that logs in memory.
    public class BotLogger : IBotLogger
    {
        /// <inheritdoc/>
        public bool Enabled { get; set; } = true;

        /// <inheritdoc/>
        public string ExecutingBlock { get; set; } = "Unknown";

        private readonly List<BotLoggerEntry> entries = new();

        /// <inheritdoc/>
        public event EventHandler<BotLoggerEntry> NewEntry;

        /// <inheritdoc/>
        public IEnumerable<BotLoggerEntry> Entries
        {
            get
            {
                lock (entries)
                {
                    // Make a copy of the list so it's thread safe
                    return entries.ToList();
                }
            }
        }

        /// <inheritdoc/>
        public void LogObject(object obj, string color = "#fff", bool canViewAsHtml = false)
        {
            var message = obj switch
            {
                List<string> list => list.AsString(),
                Dictionary<string, string> dict => dict.AsString(),
                byte[] bytes => bytes.AsString(),
                float f => f.AsString(),
                _ => obj.ToString()
            };

            Log(message, color, canViewAsHtml);
        }

        /// <inheritdoc/>
        public void Log(string message, string color = "#fff", bool canViewAsHtml = false)
        {
            if (!Enabled)
                return;

            var entry = new BotLoggerEntry
            {
                Message = message,
                Color = color,
                CanViewAsHtml = canViewAsHtml
            };

            lock (entries)
            {
                entries.Add(entry);
            }
            
            NewEntry?.Invoke(this, entry);
        }

        /// <inheritdoc/>
        public void Log(IEnumerable<string> enumerable, string color = "#fff", bool canViewAsHtml = false)
        {
            if (!Enabled)
                return;

            var entry = new BotLoggerEntry
            {
                Message = string.Join(Environment.NewLine, enumerable),
                Color = color,
                CanViewAsHtml = canViewAsHtml
            };

            lock (entries)
            {
                entries.Add(entry);
            }
            
            NewEntry?.Invoke(this, entry);
        }

        /// <inheritdoc/>
        public void LogHeader([CallerMemberName] string caller = null)
        {
            // Do not log if called by lolicode
            if (!Enabled || ExecutingBlock == "LoliCode")
                return;

            var callingMethod = new StackFrame(1).GetMethod();
            var attribute = callingMethod.GetCustomAttribute<Block>();

            if (attribute is { name: { } })
                caller = attribute.name;

            var entry = new BotLoggerEntry
            {
                Message = $">> {ExecutingBlock} ({caller}) <<",
                Color = LogColors.ChromeYellow
            };

            lock (entries)
            {
                entries.Add(new BotLoggerEntry { Message = string.Empty });
                entries.Add(entry);
            }
            
            NewEntry?.Invoke(this, entry);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (entries)
            {
                entries.Clear();
            }
        }
    }
}
