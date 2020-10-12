using RuriLib.Functions.Parsing;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Functions.Parsing
{
    public class JsonParserTests
    {
        private readonly string jsonObject = "{ \"key\": \"value\" }";
        private readonly string jsonArray = "[ \"elem1\", \"elem2\" ]";

        [Fact]
        public void GetValuesByKey_GetStringFromObject_ReturnValue()
        {
            var match = JsonParser.GetValuesByKey(jsonObject, "key").FirstOrDefault();
            Assert.Equal("value", match);
        }

        [Fact]
        public void GetValuesByKey_MissingKey_MatchNothing()
        {
            var match = JsonParser.GetValuesByKey(jsonObject, "dummy").FirstOrDefault();
            Assert.Null(match);
        }

        [Fact]
        public void GetValuesByKey_GetStringsFromArray_ReturnValues()
        {
            var match = JsonParser.GetValuesByKey(jsonArray, "[*]").ToArray();
            Assert.Equal(new string[] { "elem1", "elem2" }, match);
        }
    }
}
