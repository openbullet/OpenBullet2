using RuriLib.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RuriLib.Logging
{
    public class BotLogger : IBotLogger
    {
        private List<BotLoggerEntry> entries = new List<BotLoggerEntry>();
        public IEnumerable<BotLoggerEntry> Entries => entries;

        public void Log(string message, string color = "#fff", bool canViewAsHtml = false)
        {
            entries.Add(new BotLoggerEntry
            {
                Message = message,
                Color = color,
                CanViewAsHtml = canViewAsHtml
            });
        }

        public void Log(IEnumerable<string> enumerable, string color = "#fff", bool canViewAsHtml = false)
        {
            entries.Add(new BotLoggerEntry
            {
                Message = string.Join(Environment.NewLine, enumerable),
                Color = color,
                CanViewAsHtml = canViewAsHtml
            });
        }

        public void LogHeader([CallerMemberName] string caller = null)
        {
            var callingMethod = new StackFrame(1).GetMethod();
            var attribute = callingMethod.GetCustomAttribute<Block>();

            if (attribute != null && attribute.name != null)
                caller = attribute.name;

            entries.Add(new BotLoggerEntry
            {
                Message = $">> {caller} <<",
                Color = LogColors.ChromeYellow
            });
        }

        public void Clear()
        {
            entries.Clear();
        }
    }
}
