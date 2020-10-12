using RuriLib.Helpers;
using System;
using Xunit;

namespace RuriLib.Tests.Helpers
{
    public class VariableNamesTests
    {
        [Fact]
        public void MakeValid_Null_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => VariableNames.MakeValid(null));
        }

        [Fact]
        public void MakeValid_Empty_Random()
        {
            Assert.True(VariableNames.IsValid(VariableNames.MakeValid(string.Empty)));
        }

        [Fact]
        public void MakeValid_AllInvalid_Random()
        {
            Assert.True(VariableNames.IsValid(VariableNames.MakeValid("$<&/")));
        }

        [Fact]
        public void MakeValid_SomeInvalid_OnlyValid()
        {
            Assert.Equal("hello", VariableNames.MakeValid("!h£ello?"));
        }

        [Fact]
        public void MakeValid_StartingNumber_StartingUnderscore()
        {
            Assert.Equal("_2pac", VariableNames.MakeValid("2pac"));
        }

        [Fact]
        public void MakeValid_Dots_AllValid()
        {
            Assert.Equal("data.SOURCE", VariableNames.MakeValid("data.SOURCE"));
        }
    }
}
