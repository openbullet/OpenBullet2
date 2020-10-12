using RuriLib.Functions.Parsing;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Functions.Parsing
{
    public class RegexParserTests
    {
        private readonly string oneLineString = "The cat is on the table";

        [Fact]
        public void MatchGroupsToString_SingleMatch_GetFullMatch()
        {
            var match = RegexParser.MatchGroupsToString(oneLineString, @"The ([^ ]*) is", "[0]").FirstOrDefault();
            Assert.Equal("The cat is", match);
        }

        [Fact]
        public void MatchGroupsToString_SingleMatch_GetGroups()
        {
            var match = RegexParser.MatchGroupsToString(oneLineString, @"The ([^ ]*) is on the (.*)$", "[1] - [2]").FirstOrDefault();
            Assert.Equal("cat - table", match);
        }

        [Fact]
        public void MatchGroupsToString_SingleMatchEmptyOutputFormat_ReturnEmptyString()
        {
            var match = RegexParser.MatchGroupsToString(oneLineString, @"The ([^ ]*) is", string.Empty).FirstOrDefault();
            Assert.Equal(string.Empty, match);
        }

        [Fact]
        public void MatchGroupsToString_NoMatches_MatchNothing()
        {
            var match = RegexParser.MatchGroupsToString(oneLineString, @"John went to the (.*)$", "[0]").FirstOrDefault();
            Assert.Null(match);
        }
    }
}
