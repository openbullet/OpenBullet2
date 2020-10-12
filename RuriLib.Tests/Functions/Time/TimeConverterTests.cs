using RuriLib.Functions.Time;
using System;
using Xunit;

namespace RuriLib.Tests.Functions.Time
{
    public class TimeConverterTests
    {
        private readonly DateTime recentDate = new DateTime(2020, 4, 18, 0, 0, 0, DateTimeKind.Utc);
        private readonly long recentUnixTimeSeconds = 1587168000;
        private readonly string recentISO8601Date = "2020-04-18T00:00:00.000Z";

        [Fact]
        public void ToUnixTime_Seconds_OutputSeconds()
        {
            long unixTime = recentDate.ToUnixTime();
            Assert.Equal(recentUnixTimeSeconds, unixTime);
        }

        [Fact]
        public void ToUnixTime_Milliseconds_OutputMilliseconds()
        {
            long unixTime = recentDate.ToUnixTime(true);
            Assert.Equal(recentUnixTimeSeconds * 1000, unixTime);
        }

        [Fact]
        public void ToDateTimeUtc_Seconds_OutputCorrectDate()
        {
            DateTime dateTime = recentUnixTimeSeconds.ToDateTimeUtc();
            Assert.Equal(recentDate.ToUniversalTime(), dateTime);
        }

        [Fact]
        public void ToDateTimeUtc_Milliseconds_OutputCorrectDate()
        {
            DateTime dateTime = (recentUnixTimeSeconds * 1000).ToDateTimeUtc(true);
            Assert.Equal(recentDate.ToUniversalTime(), dateTime);
        }

        [Fact]
        public void ToDateTime_FullTimeString_OutputCorrectDate()
        {
            DateTime dateTime = recentISO8601Date.ToDateTime("yyyy-MM-ddTHH:mm:ss.fffZ");
            Assert.Equal(recentDate, dateTime);
        }

        [Fact]
        public void ToISO8601_Normal_OutputISO8601String()
        {
            string iso = recentDate.ToISO8601();
            Assert.Equal(recentISO8601Date, iso);
        }
    }
}
