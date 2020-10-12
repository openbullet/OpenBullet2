using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Helpers.LoliCode
{
    public class LoliCodeParserTests
    {
        [Fact]
        public void ParseLiteral_NormalLiteral_Parse()
        {
            string input = "\"hello\" how are you";
            Assert.Equal("hello", LineParser.ParseLiteral(ref input));
            Assert.Equal("how are you", input);
        }

        [Fact]
        public void ParseLiteral_LiteralWithEscapedQuotes_Parse()
        {
            string input = "\"I \\\"escape\\\" quotes\"";
            Assert.Equal("I \"escape\" quotes", LineParser.ParseLiteral(ref input));
        }

        [Fact]
        public void ParseLiteral_LiteralWithDoubleEscaping_Parse()
        {
            string input = "\"I \\\\\"don't\\\\\" escape quotes\"";
            Assert.Equal("I \\", LineParser.ParseLiteral(ref input));
        }

        [Fact]
        public void ParseInt_NormalInt_Parse()
        {
            string input = "42 is the answer";
            Assert.Equal(42, LineParser.ParseInt(ref input));
            Assert.Equal("is the answer", input);
        }

        [Fact]
        public void ParseFloat_NormalFloat_Parse()
        {
            string input = "3.14 is not pi";
            Assert.Equal(3.14f, LineParser.ParseFloat(ref input));
            Assert.Equal("is not pi", input);
        }

        [Fact]
        public void ParseList_EmptyList_Parse()
        {
            string input = "[] such emptiness";
            Assert.Empty(LineParser.ParseList(ref input));
            Assert.Equal("such emptiness", input);
        }

        [Fact]
        public void ParseList_EmptyListWithSpacesInside_Parse()
        {
            string input = "[ ]";
            Assert.Empty(LineParser.ParseList(ref input));
        }

        [Fact]
        public void ParseList_SingleElementList_Parse()
        {
            string input = "[\"one\"]";
            Assert.Equal(new string[] { "one" }, LineParser.ParseList(ref input).ToArray());
        }

        [Fact]
        public void ParseList_MultiElementList_Parse()
        {
            string input = "[\"one\", \"two\", \"three\"]";
            Assert.Equal(new string[] { "one", "two", "three" }, LineParser.ParseList(ref input).ToArray());
        }

        [Fact]
        public void ParseByteArray_NormalBase64_Parse()
        {
            string input = "/wA= is my byte array";
            Assert.Equal(new byte[] { 0xFF, 0x00 }, LineParser.ParseByteArray(ref input));
            Assert.Equal("is my byte array", input);
        }

        [Fact]
        public void ParseBool_NormalBool_Parse()
        {
            string input = "True indeed";
            Assert.True(LineParser.ParseBool(ref input));
            Assert.Equal("indeed", input);
        }

        [Fact]
        public void ParseDictionary_EmptyDictionary_Parse()
        {
            string input = "{} such emptiness";
            Assert.Empty(LineParser.ParseDictionary(ref input));
            Assert.Equal("such emptiness", input);
        }

        [Fact]
        public void ParseDictionary_EmptyDictionaryWithSpacesInside_Parse()
        {
            string input = "{ }";
            Assert.Empty(LineParser.ParseDictionary(ref input));
        }

        [Fact]
        public void ParseDictionary_SingleEntry_Parse()
        {
            string input = "{ (\"key1\", \"value1\") }";
            var dict = LineParser.ParseDictionary(ref input);
            Assert.Equal("value1", dict["key1"]);
        }

        [Fact]
        public void ParseDictionary_MultiEntry_Parse()
        {
            string input = "{ (\"key1\", \"value1\"), (\"key2\", \"value2\") }";
            var dict = LineParser.ParseDictionary(ref input);
            Assert.Equal("value1", dict["key1"]);
            Assert.Equal("value2", dict["key2"]);
        }

        [Fact]
        public void ParseSetting_FixedStringSetting_Parse()
        {
            var block = new BlockFactory().GetBlock<AutoBlockInstance>("ParseBetweenStrings");
            var leftDelimSetting = block.Settings.First(s => s.Name == "leftDelim");

            string input = "leftDelim = \"myValue\"";
            LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
            Assert.Equal(SettingInputMode.Fixed, leftDelimSetting.InputMode);
            Assert.IsType<StringSetting>(leftDelimSetting.FixedSetting);
            Assert.Equal("myValue", (leftDelimSetting.FixedSetting as StringSetting).Value);
        }

        [Fact]
        public void ParseSetting_Variable_Parse()
        {
            var block = new BlockFactory().GetBlock<AutoBlockInstance>("ParseBetweenStrings");
            var leftDelimSetting = block.Settings.First(s => s.Name == "leftDelim");

            string input = "leftDelim = @myVariable";
            LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
            Assert.Equal(SettingInputMode.Variable, leftDelimSetting.InputMode);
            Assert.Equal("myVariable", leftDelimSetting.InputVariableName);
        }

        [Fact]
        public void ParseSetting_InterpolatedString_Parse()
        {
            var block = new BlockFactory().GetBlock<AutoBlockInstance>("ParseBetweenStrings");
            var leftDelimSetting = block.Settings.First(s => s.Name == "leftDelim");

            string input = "leftDelim = $\"my <interp> string\"";
            LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
            Assert.Equal(SettingInputMode.Interpolated, leftDelimSetting.InputMode);
            Assert.IsType<InterpolatedStringSetting>(leftDelimSetting.InterpolatedSetting);
            Assert.Equal("my <interp> string", ((InterpolatedStringSetting)leftDelimSetting.InterpolatedSetting).Value);
        }
    }
}
