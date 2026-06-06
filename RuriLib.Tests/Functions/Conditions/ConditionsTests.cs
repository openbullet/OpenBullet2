using RuriLib.Models.Conditions.Comparisons;
using System;
using System.Collections.Generic;
using ConditionMethods = RuriLib.Functions.Conditions.Conditions;
using Xunit;

namespace RuriLib.Tests.Functions.Conditions;

public class ConditionsTests
{
    [Fact]
    public void Check_BoolComparisons_ReturnExpectedValues()
    {
        Assert.True(ConditionMethods.Check(true, BoolComparison.Is, true));
        Assert.True(ConditionMethods.Check(true, BoolComparison.IsNot, false));
        Assert.False(ConditionMethods.Check(false, BoolComparison.Is, true));
    }

    [Fact]
    public void Check_StringNullAwareComparisons_ReturnExpectedValues()
    {
        Assert.True(ConditionMethods.Check("value", StrComparison.Exists, null));
        Assert.True(ConditionMethods.Check(null, StrComparison.DoesNotExist, null));
        Assert.True(ConditionMethods.Check(null, StrComparison.EqualTo, null));
        Assert.True(ConditionMethods.Check(null, StrComparison.NotEqualTo, "value"));
    }

    [Fact]
    public void Check_StringContentComparisons_ReturnExpectedValues()
    {
        Assert.True(ConditionMethods.Check("hello world", StrComparison.Contains, "world"));
        Assert.True(ConditionMethods.Check("hello world", StrComparison.DoesNotContain, "missing"));
        Assert.True(ConditionMethods.Check("abc123", StrComparison.MatchesRegex, @"^[a-z]+\d+$"));
        Assert.True(ConditionMethods.Check("abc", StrComparison.DoesNotMatchRegex, @"\d+"));
    }

    [Fact]
    public void Check_StringContentComparisonWithNullTerm_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
            ConditionMethods.Check(null, StrComparison.Contains, "value"));

    [Fact]
    public void Check_ListComparisons_ReturnExpectedValues()
    {
        var list = new List<string> { "alpha", "beta" };

        Assert.True(ConditionMethods.Check(list, ListComparison.Exists, null));
        Assert.True(ConditionMethods.Check(null, ListComparison.DoesNotExist, null));
        Assert.True(ConditionMethods.Check(list, ListComparison.Contains, "beta"));
        Assert.True(ConditionMethods.Check(list, ListComparison.DoesNotContain, "gamma"));
    }

    [Fact]
    public void Check_NumericComparisons_ReturnExpectedValues()
    {
        Assert.True(ConditionMethods.Check(3, NumComparison.LessThan, 5));
        Assert.True(ConditionMethods.Check(5, NumComparison.GreaterThanOrEqualTo, 5));
        Assert.True(ConditionMethods.Check(1.5f, NumComparison.EqualTo, 1.5f));
        Assert.True(ConditionMethods.Check(TimeSpan.FromSeconds(2), NumComparison.LessThan, TimeSpan.FromSeconds(3)));
    }

    [Fact]
    public void Check_DictionaryComparisons_ReturnExpectedValues()
    {
        var dict = new Dictionary<string, string> { ["alpha"] = "one" };

        Assert.True(ConditionMethods.Check(dict, DictComparison.Exists, null));
        Assert.True(ConditionMethods.Check(null, DictComparison.DoesNotExist, null));
        Assert.True(ConditionMethods.Check(dict, DictComparison.HasKey, "alpha"));
        Assert.True(ConditionMethods.Check(dict, DictComparison.DoesNotHaveKey, "beta"));
        Assert.True(ConditionMethods.Check(dict, DictComparison.HasValue, "one"));
        Assert.True(ConditionMethods.Check(dict, DictComparison.DoesNotHaveValue, "two"));
    }

    [Fact]
    public void Check_UnsupportedComparison_Throws()
        => Assert.Throws<ArgumentException>(() =>
            ConditionMethods.Check(true, (BoolComparison)999, false));
}
