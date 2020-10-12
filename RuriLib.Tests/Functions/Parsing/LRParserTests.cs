using RuriLib.Functions.Parsing;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Functions.Parsing
{
    public class LRParserTests
    {
        private readonly string oneLineString = "The cat is on the table";
        private readonly string oneLineTwoMatchesString = "The cat is fat, the dog is lazy";

        [Fact]
        public void ParseBetweenStrings_OneLineStringCaseSensitive_GetText()
        {
            string match = LRParser.ParseBetween(oneLineString, "The ", " is").FirstOrDefault();
            Assert.Equal("cat", match);
        }

        [Fact]
        public void ParseBetweenStrings_OneLineStringCaseSensitive_MatchNothing()
        {
            string match = LRParser.ParseBetween(oneLineString, "THE ", " IS").FirstOrDefault();
            Assert.Null(match);
        }

        [Fact]
        public void ParseBetweenStrings_OneLineStringCaseInsensitive_GetText()
        {
            string match = LRParser.ParseBetween(oneLineString, "THE ", " IS", false).FirstOrDefault();
            Assert.Equal("cat", match);
        }

        [Fact]
        public void ParseBetweenStrings_OneLineStringManyMatches_GetText()
        {
            var matches = LRParser.ParseBetween(oneLineTwoMatchesString, "the ", " is", false).ToArray();
            Assert.Equal(new string[] { "cat", "dog" }, matches);
        }

        [Fact]
        public void ParseBetweenStrings_LeftDelimEmpty_ParseFromBeginning()
        {
            string match = LRParser.ParseBetween(oneLineString, string.Empty, " is").FirstOrDefault();
            Assert.Equal("The cat", match);
        }

        [Fact]
        public void ParseBetweenStrings_RightDelimEmpty_ParseUntilEnd()
        {
            string match = LRParser.ParseBetween(oneLineString, "is ", string.Empty).FirstOrDefault();
            Assert.Equal("on the table", match);
        }

        [Fact]
        public void ParseBetweenStrings_BothDelimsEmpty_ParseEntireInput()
        {
            string match = LRParser.ParseBetween(oneLineString, string.Empty, string.Empty).FirstOrDefault();
            Assert.Equal(oneLineString, match);
        }

        [Fact]
        public void ParseBetweenStrings_LeftDelimNotFound_MatchNothing()
        {
            string match = LRParser.ParseBetween(oneLineString, "John", "table").FirstOrDefault();
            Assert.Null(match);
        }

        [Fact]
        public void ParseBetweenStrings_RightDelimNotFound_MatchNothing()
        {
            string match = LRParser.ParseBetween(oneLineString, "cat", "John").FirstOrDefault();
            Assert.Null(match);
        }

        [Fact]
        public void ParseBetweenStrings_BothDelimsNotFound_MatchNothing()
        {
            string match = LRParser.ParseBetween(oneLineString, "John", "Mary").FirstOrDefault();
            Assert.Null(match);
        }
    }
}
