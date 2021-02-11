using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using Xunit;

namespace RuriLib.Tests.Helpers.LoliCode
{
    public class LoliCodeParserTests
    {
        [Fact]
        public void ParseLiteral_NormalLiteral_Parse()
        {
            var input = "\"hello\" how are you";
            Assert.Equal("hello", LineParser.ParseLiteral(ref input));
            Assert.Equal("how are you", input);
        }

        [Fact]
        public void ParseLiteral_LiteralWithEscapedQuotes_Parse()
        {
            var input = "\"I \\\"escape\\\" quotes\"";
            Assert.Equal("I \"escape\" quotes", LineParser.ParseLiteral(ref input));
        }

        [Fact]
        public void ParseLiteral_LiteralWithDoubleEscaping_Parse()
        {
            var input = "\"I \\\\\"don't\\\\\" escape quotes\"";
            Assert.Equal("I \\", LineParser.ParseLiteral(ref input));
        }

        [Fact]
        public void ParseInt_NormalInt_Parse()
        {
            var input = "42 is the answer";
            Assert.Equal(42, LineParser.ParseInt(ref input));
            Assert.Equal("is the answer", input);
        }

        [Fact]
        public void ParseFloat_NormalFloat_Parse()
        {
            var input = "3.14 is not pi";
            Assert.Equal(3.14f, LineParser.ParseFloat(ref input));
            Assert.Equal("is not pi", input);
        }

        [Fact]
        public void ParseList_EmptyList_Parse()
        {
            var input = "[] such emptiness";
            Assert.Empty(LineParser.ParseList(ref input));
            Assert.Equal("such emptiness", input);
        }

        [Fact]
        public void ParseList_EmptyListWithSpacesInside_Parse()
        {
            var input = "[ ]";
            Assert.Empty(LineParser.ParseList(ref input));
        }

        [Fact]
        public void ParseList_SingleElementList_Parse()
        {
            var input = "[\"one\"]";
            Assert.Equal(new string[] { "one" }, LineParser.ParseList(ref input).ToArray());
        }

        [Fact]
        public void ParseList_MultiElementList_Parse()
        {
            var input = "[\"one\", \"two\", \"three\"]";
            Assert.Equal(new string[] { "one", "two", "three" }, LineParser.ParseList(ref input).ToArray());
        }

        [Fact]
        public void ParseByteArray_NormalBase64_Parse()
        {
            var input = "/wA= is my byte array";
            Assert.Equal(new byte[] { 0xFF, 0x00 }, LineParser.ParseByteArray(ref input));
            Assert.Equal("is my byte array", input);
        }

        [Fact]
        public void ParseBool_NormalBool_Parse()
        {
            var input = "True indeed";
            Assert.True(LineParser.ParseBool(ref input));
            Assert.Equal("indeed", input);
        }

        [Fact]
        public void ParseDictionary_EmptyDictionary_Parse()
        {
            var input = "{} such emptiness";
            Assert.Empty(LineParser.ParseDictionary(ref input));
            Assert.Equal("such emptiness", input);
        }

        [Fact]
        public void ParseDictionary_EmptyDictionaryWithSpacesInside_Parse()
        {
            var input = "{ }";
            Assert.Empty(LineParser.ParseDictionary(ref input));
        }

        [Fact]
        public void ParseDictionary_SingleEntry_Parse()
        {
            var input = "{ (\"key1\", \"value1\") }";
            var dict = LineParser.ParseDictionary(ref input);
            Assert.Equal("value1", dict["key1"]);
        }

        [Fact]
        public void ParseDictionary_MultiEntry_Parse()
        {
            var input = "{ (\"key1\", \"value1\"), (\"key2\", \"value2\") }";
            var dict = LineParser.ParseDictionary(ref input);
            Assert.Equal("value1", dict["key1"]);
            Assert.Equal("value2", dict["key2"]);
        }

        [Fact]
        public void ParseSetting_FixedStringSetting_Parse()
        {
            var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
            var valueSetting = block.Settings["value"];

            var input = "value = \"myValue\"";
            LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
            Assert.Equal(SettingInputMode.Fixed, valueSetting.InputMode);
            Assert.IsType<StringSetting>(valueSetting.FixedSetting);
            Assert.Equal("myValue", (valueSetting.FixedSetting as StringSetting).Value);
        }

        [Fact]
        public void ParseSetting_Variable_Parse()
        {
            var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
            var valueSetting = block.Settings["value"];

            var input = "value = @myVariable";
            LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
            Assert.Equal(SettingInputMode.Variable, valueSetting.InputMode);
            Assert.Equal("myVariable", valueSetting.InputVariableName);
        }

        [Fact]
        public void ParseSetting_InterpolatedString_Parse()
        {
            var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
            var valueSetting = block.Settings["value"];

            var input = "value = $\"my <interp> string\"";
            LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
            Assert.Equal(SettingInputMode.Interpolated, valueSetting.InputMode);
            Assert.IsType<InterpolatedStringSetting>(valueSetting.InterpolatedSetting);
            Assert.Equal("my <interp> string", ((InterpolatedStringSetting)valueSetting.InterpolatedSetting).Value);
        }
    }
}
