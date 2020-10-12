using RuriLib.Functions.Parsing;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Functions.Parsing
{
    public class HtmlParserTests
    {
        private readonly string htmlPage = @"
<html>
<body>
<p>text <strong>123</strong></p>
<ul>
    <li><p id='testid' testattr='testvalue'>innertext1</p></li>
    <li><p>innertext2</p></li>
<ul/>
</body>
</html>
";

        [Fact]
        public void QueryAttributeAll_SingleElementQueryInnerHTML_GetText()
        {
            var match = HtmlParser.QueryAttributeAll(htmlPage, "#testid", "innerHTML").FirstOrDefault();
            Assert.Equal("innertext1", match);
        }

        [Fact]
        public void QueryAttributeAll_SingleElementQueryOuterHTML_GetText()
        {
            var match = HtmlParser.QueryAttributeAll(htmlPage, "strong", "outerHTML").FirstOrDefault();
            Assert.Equal("<strong>123</strong>", match);
        }

        [Fact]
        public void QueryAttributeAll_SingleElementQueryInnerText_GetText()
        {
            var match = HtmlParser.QueryAttributeAll(htmlPage, "body p", "innerText").FirstOrDefault();
            Assert.Equal("text 123", match);
        }

        [Fact]
        public void QueryAttributeAll_SingleElementNonExistant_MatchNothing()
        {
            var match = HtmlParser.QueryAttributeAll(htmlPage, "#nonexistant", "innerHTML").FirstOrDefault();
            Assert.Null(match);
        }

        [Fact]
        public void QueryAttributeAll_SingleElementQueryAttribute_GetText()
        {
            var match = HtmlParser.QueryAttributeAll(htmlPage, "#testid", "testattr").FirstOrDefault();
            Assert.Equal("testvalue", match);
        }

        [Fact]
        public void QueryAttributeAll_ManyElementsQueryAttribute_GetText()
        {
            var matches = HtmlParser.QueryAttributeAll(htmlPage, "ul li p", "innerHTML").ToArray();
            Assert.Equal(new string[] { "innertext1", "innertext2" }, matches);
        }
    }
}
