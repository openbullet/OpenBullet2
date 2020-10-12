using System;
using System.Globalization;

namespace RuriLib.Functions.Time
{
    /// <summary>
    /// Provides methods to work with dates and times.
    /// </summary>
    public static class TimeConverter
    {
        /// <summary>
        /// Converts a DateTime to unix time seconds.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>The seconds that passed since Jan 1st 1970.</returns>
        public static long ToUnixTime(this DateTime dateTime, bool outputMilliseconds = false)
        {
            TimeSpan dt = dateTime.Subtract(new DateTime(1970, 1, 1));
            
            return outputMilliseconds
                ? (long)dt.TotalMilliseconds
                : (long)dt.TotalSeconds;
        }

        /// <summary>
        /// Converts a unix time to a universal DateTime.
        /// </summary>
        /// <param name="unixTime">The unix time in seconds or milliseconds</param>
        /// <returns>A DateTime.</returns>
        public static DateTime ToDateTimeUtc(this long unixTime, bool isMilliseconds = false)
        {
            if (!isMilliseconds) unixTime *= 1000;
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(unixTime)
                .ToUniversalTime();
        }

        /// <summary>
        /// Converts a time string to a DateTime.
        /// </summary>
        /// <param name="time">The string to convert</param>
        /// <param name="format">The string's format</param>
        /// <returns>A DateTime.</returns>
        public static DateTime ToDateTime(this string time, string format)
        {
            return DateTime.ParseExact(time, format, new CultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces)
                .ToUniversalTime();
        }

        /// <summary>
        /// Converts a DateTime to an ISO8601 time string.
        /// </summary>
        /// <param name="dateTime">A DateTime</param>
        /// <returns>An ISO8601 time string.</returns>
        public static string ToISO8601(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ");
        }
    }
}
