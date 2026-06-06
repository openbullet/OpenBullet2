using RuriLib.Attributes;
using RuriLib.Functions.Time;
using RuriLib.Models.Bots;
using System;

namespace RuriLib.Blocks.Functions.Time;

/// <summary>
/// Blocks for working with dates and times.
/// </summary>
[BlockCategory("Time", "Blocks for working with dates and times", "#9acd32")]
public static class Methods
{
    /// <summary>
    /// Gets the current unix time in seconds.
    /// </summary>
    public static int CurrentUnixTime(BotData data, bool useUtc = false)
        => Convert.ToInt32(CurrentUnixTimeLong(data, useUtc, outputMilliseconds: false));

    /// <summary>
    /// Gets the current unix time in seconds or milliseconds.
    /// </summary>
    [Block("Gets the current unix time in seconds or milliseconds", id = nameof(CurrentUnixTime), name = "Current Unix Time")]
    public static long CurrentUnixTimeLong(BotData data, bool useUtc = false, bool outputMilliseconds = false)
    {
        data.Logger.LogHeader();

        var dateTime = useUtc ? DateTime.UtcNow : DateTime.Now;
        var time = dateTime.ToUnixTime(outputMilliseconds);
        data.Logger.Log($"Current unix time: {time}");
        return time;
    }

    /// <summary>
    /// Gets the current unix time in milliseconds.
    /// </summary>
    public static long CurrentUnixTimeMilliseconds(BotData data, bool useUtc = false)
        => CurrentUnixTimeLong(data, useUtc, outputMilliseconds: true);

    /// <summary>
    /// Converts a unix time to a formatted datetime string.
    /// </summary>
    [Block("Converts a unix time to a formatted datetime string")]
    public static string UnixTimeToDate(BotData data, [Variable] long unixTime, string format = "yyyy-MM-dd:HH-mm-ss",
        bool inputMilliseconds = false)
    {
        data.Logger.LogHeader();

        var date = unixTime.ToDateTimeUtc(inputMilliseconds).ToString(format);
        data.Logger.Log($"Formatted datetime: {date}");
        return date;
    }

    /// <summary>
    /// Converts a unix time to a formatted datetime string.
    /// </summary>
    public static string UnixTimeToDate(BotData data, [Variable] int unixTime, string format = "yyyy-MM-dd:HH-mm-ss")
        => UnixTimeToDate(data, (long)unixTime, format, inputMilliseconds: false);

    /// <summary>
    /// Converts a unix time in milliseconds to a formatted datetime string.
    /// </summary>
    public static string UnixTimeMillisecondsToDate(BotData data, [Variable] long unixTime, string format = "yyyy-MM-dd:HH-mm-ss")
        => UnixTimeToDate(data, unixTime, format, inputMilliseconds: true);

    /// <summary>
    /// Parses a unix time from a formatted datetime string.
    /// </summary>
    public static int DateToUnixTime(BotData data, [Variable] string datetime, string format)
        => Convert.ToInt32(DateToUnixTimeLong(data, datetime, format, outputMilliseconds: false));

    /// <summary>
    /// Parses a unix time in seconds or milliseconds from a formatted datetime string.
    /// </summary>
    [Block("Parses a unix time in seconds or milliseconds from a formatted datetime string", id = nameof(DateToUnixTime), name = "Date To Unix Time")]
    public static long DateToUnixTimeLong(BotData data, [Variable] string datetime, string format, bool outputMilliseconds = false)
    {
        data.Logger.LogHeader();

        var time = datetime.ToDateTime(format).ToUnixTime(outputMilliseconds);
        data.Logger.Log($"Unix time: {time}");
        return time;
    }

    /// <summary>
    /// Parses a unix time in milliseconds from a formatted datetime string.
    /// </summary>
    public static long DateToUnixTimeMilliseconds(BotData data, [Variable] string datetime, string format)
        => DateToUnixTimeLong(data, datetime, format, outputMilliseconds: true);

    /// <summary>
    /// Converts a unix time to an ISO8601 datetime string.
    /// </summary>
    [Block("Converts a unix time to an ISO8601 datetime string")]
    public static string UnixTimeToISO8601(BotData data, [Variable] long unixTime)
    {
        data.Logger.LogHeader();

        var iso = unixTime.ToDateTimeUtc().ToISO8601();
        data.Logger.Log($"ISO8601 datetime: {iso}");
        return iso;
    }

    /// <summary>
    /// Converts a unix time to an ISO8601 datetime string.
    /// </summary>
    public static string UnixTimeToISO8601(BotData data, [Variable] int unixTime)
        => UnixTimeToISO8601(data, (long)unixTime);
}
