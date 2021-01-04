using RuriLib.Models.Data.Rules;
using Xunit;

namespace RuriLib.Tests.Models.Data
{
    public class DataRuleTests
    {
        [Fact]
        public void IsSatisfied_Contains_True()
        {
            var rule = new SimpleDataRule
            {
                Comparison = StringRule.Contains,
                StringToCompare = "abc",
                Invert = false
            };

            Assert.True(rule.IsSatisfied("abcdef"));
        }

        [Fact]
        public void IsSatisfied_ContainsInvert_False()
        {
            var rule = new SimpleDataRule
            {
                Comparison = StringRule.Contains,
                StringToCompare = "abc",
                Invert = true
            };

            Assert.False(rule.IsSatisfied("abcdef"));
        }

        [Fact]
        public void IsSatisfied_ContainsCaseInsensitive_False()
        {
            var rule = new SimpleDataRule
            {
                Comparison = StringRule.Contains,
                StringToCompare = "ABC",
                CaseSensitive = false,
                Invert = false
            };

            Assert.True(rule.IsSatisfied("abcdef"));
        }

        [Fact]
        public void IsSatisfied_ContainsAll_True()
        {
            var rule = new SimpleDataRule
            {
                Comparison = StringRule.ContainsAll,
                StringToCompare = "acf",
                Invert = false
            };

            Assert.True(rule.IsSatisfied("abcdef"));
        }

        [Fact]
        public void IsSatisfied_ContainsAny_True()
        {
            var rule = new SimpleDataRule
            {
                Comparison = StringRule.ContainsAny,
                StringToCompare = "a78",
                Invert = false
            };

            Assert.True(rule.IsSatisfied("abcdef"));
        }
    }
}
