using RuriLib.Attributes;
using RuriLib.Functions.Time;
using RuriLib.Models.Bots;
using System;

namespace RuriLib.Blocks.Functions.Time
{
    [BlockCategory("Time", "Blocks for working with dates and times", "#9acd32")]
    public static class Methods
    {
        [Block("Gets the current unix time in seconds")]
        public static int CurrentUnixTime(BotData data, bool useUtc = false)
        {
            var dateTime = useUtc ? DateTime.UtcNow : DateTime.Now;
            var time = (int)dateTime.ToUnixTime();
            data.Logger.LogHeader();
            data.Logger.Log($"Current unix time: {time}");
            return time;
        }

        [Block("Converts a unix time to a formatted datetime string")]
        public static string UnixTimeToDate(BotData data, [Variable] int unixTime, string format = "yyyy-MM-dd:HH-mm-ss")
        {
            var date = ((long)unixTime).ToDateTimeUtc().ToString(format);
            data.Logger.LogHeader();
            data.Logger.Log($"Formatted datetime: {date}");
            return date;
        }

        [Block("Parses a unix time from a formatted datetime string")]
        public static int DateToUnixTime(BotData data, [Variable] string datetime, string format)
        {
            var time = (int)datetime.ToDateTime(format).ToUnixTime();
            data.Logger.LogHeader();
            data.Logger.Log($"Unix time: {time}");
            return time;
        }

        [Block("Converts a unix time to an ISO8601 datetime string")]
        public static string UnixTimeToISO8601(BotData data, [Variable] int unixTime)
        {
            var iso = ((long)unixTime).ToDateTimeUtc().ToISO8601();
            data.Logger.LogHeader();
            data.Logger.Log($"ISO8601 datetime: {iso}");
            return iso;
        }
    }
}
