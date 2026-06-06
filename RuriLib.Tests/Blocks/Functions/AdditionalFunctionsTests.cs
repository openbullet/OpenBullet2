using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Providers.Proxies;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using ByteArrayMethods = RuriLib.Blocks.Functions.ByteArray.Methods;
using ConditionsMethods = RuriLib.Blocks.Conditions.Methods;
using ConstantsMethods = RuriLib.Blocks.Functions.Constants.Methods;
using CryptoMethods = RuriLib.Blocks.Functions.Crypto.Methods;
using DictionaryMethods = RuriLib.Blocks.Functions.Dictionary.Methods;
using FloatMethods = RuriLib.Blocks.Functions.Float.Methods;
using HashFunction = RuriLib.Functions.Crypto.HashFunction;
using IntegerMethods = RuriLib.Blocks.Functions.Integer.Methods;

namespace RuriLib.Tests.Blocks.Functions;

public class AdditionalFunctionsTests
{
    [Fact]
    public void MergeByteArrays_AppendsSecondArrayAfterFirst()
    {
        var data = NewBotData();

        var merged = ByteArrayMethods.MergeByteArrays(data, [0x01, 0x02], [0x03, 0x04]);

        Assert.Equal([0x01, 0x02, 0x03, 0x04], merged);
    }

    [Fact]
    public void ConstantList_ReturnsClone()
    {
        var data = NewBotData();
        var source = new List<string> { "alpha", "beta" };

        var cloned = ConstantsMethods.ConstantList(data, source);
        source[0] = "changed";

        Assert.Equal(["alpha", "beta"], cloned);
    }

    [Fact]
    public void Constants_ReturnExpectedValues()
    {
        var data = NewBotData();

        Assert.Equal(42L, ConstantsMethods.ConstantInteger(data, 42L));
        Assert.Equal(42, ConstantsMethods.ConstantInteger(data, 42));
        Assert.Equal(1.5D, ConstantsMethods.ConstantFloat(data, 1.5D));
        Assert.Equal(1.5f, ConstantsMethods.ConstantFloat(data, 1.5f));
        Assert.True(ConstantsMethods.ConstantBool(data, true));
        Assert.Equal([0x01, 0x02], ConstantsMethods.ConstantByteArray(data, [0x01, 0x02]));
    }

    [Fact]
    public void RandomInteger_IncludesUpperBound()
    {
        var data = NewBotData();

        var value = IntegerMethods.RandomInteger(data, 5L, 5L);

        Assert.Equal(5L, value);
    }

    [Fact]
    public void RandomInteger_WithLargeBounds_ReturnsLong()
    {
        var data = NewBotData();

        var value = IntegerMethods.RandomInteger(data, 5_000_000_000L, 5_000_000_000L);

        Assert.Equal(5_000_000_000L, value);
    }

    [Fact]
    public void RandomFloat_WithSameBounds_ReturnsBound()
    {
        var data = NewBotData();

        var value = FloatMethods.RandomFloat(data, 2.5D, 2.5D);

        Assert.Equal(2.5D, value);
    }

    [Fact]
    public void Compute_EvaluatesExpression()
    {
        var data = NewBotData();

        var value = FloatMethods.ComputeDouble(data, "3*(2+1)");

        Assert.Equal(9D, value);
    }

    [Fact]
    public void Compute_PreservesDoublePrecision()
    {
        var data = NewBotData();

        var value = FloatMethods.ComputeDouble(data, "16777217");

        Assert.Equal(16777217D, value);
    }

    [Fact]
    public void NumericHelpers_ReturnExpectedValues()
    {
        var data = NewBotData();

        Assert.Equal(3L, FloatMethods.Ceil(data, 2.1D));
        Assert.Equal(2L, FloatMethods.Floor(data, 2.9D));
        Assert.Equal(3L, FloatMethods.RoundToInteger(data, 2.5D));
        Assert.Equal(4.5D, FloatMethods.TakeMaxFloat(data, 1.5D, 4.5D));
        Assert.Equal(1.5D, FloatMethods.TakeMinFloat(data, 1.5D, 4.5D));
        Assert.Equal(9L, IntegerMethods.TakeMaxInt(data, 4L, 9L));
        Assert.Equal(4L, IntegerMethods.TakeMinInt(data, 4L, 9L));
    }

    [Fact]
    public void CryptoWrappers_ReturnExpectedValues()
    {
        var data = NewBotData();

        var xor = CryptoMethods.XOR(data, [0x0F, 0xF0], [0xFF, 0xFF]);
        var hash = CryptoMethods.HashString(
            data,
            "hello",
            HashFunction.SHA256);
        var hmac = CryptoMethods.HmacString(
            data,
            "hello",
            Encoding.UTF8.GetBytes("key"),
            HashFunction.SHA256);
        var bcryptHash = CryptoMethods.BCryptHashGenSalt(data, "secret", rounds: 4);

        Assert.Equal([0xF0, 0x0F], xor);
        Assert.Equal("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", hash);
        Assert.Equal("9307b3b915efb5171ff14d8cb55fbcc798c6c0ef1456d66ded1a6aa723a58b7b", hmac);
        Assert.True(CryptoMethods.BCryptVerify(data, "secret", bcryptHash));
    }

    [Fact]
    public void ScryptString_ReturnsEncodedHashInsteadOfRawDerivedKey()
    {
        var data = NewBotData();

        var hashed = CryptoMethods.ScryptString(
            data,
            "test1234",
            "salt",
            iterationCount: 1024,
            blockSize: 15,
            threadCount: 1);

        Assert.Equal(
            "$s2$1024$15$1$c2FsdAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=$AoifrOvw43HdYrBT/t11pQ8gpIk0Cx9s7cg6W0SU/1o=",
            hashed);
    }

    [Fact]
    public void ScryptDeriveKey_ReturnsExpectedBrowserlingStyleHex()
    {
        var data = NewBotData();

        var derived = CryptoMethods.ScryptDeriveKey(
            data,
            "test1234",
            "salt",
            outputSize: 15,
            iterationCount: 1024,
            blockSize: 15,
            threadCount: 1);

        Assert.Equal("9c4351797ce2d8f0af5c63f20d82a6", derived);
    }

    [Fact]
    public void ScryptString_WithVeryLongSalt_TruncatesToLegacySaltLength()
    {
        var data = NewBotData();
        var salt = new string('a', 256);

        var hashed = CryptoMethods.ScryptString(data, "test1234", salt);

        Assert.Equal(
            "$s2$16384$8$1$YWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWE=$lmoJTLRaLFu78m7K19YNT4NzXKVdWrbH++n2AyVckqM=",
            hashed);
    }

    [Fact]
    public void DictionaryMethods_GetKeyReturnsEmptyStringWhenNotFound()
    {
        var data = NewBotData();

        var key = DictionaryMethods.GetKey(
            data,
            new Dictionary<string, string> { ["alpha"] = "1" },
            "missing");

        Assert.Equal(string.Empty, key);
    }

    [Fact]
    public void DictionaryMethods_AddAndRemoveByKey_UpdateDictionary()
    {
        var data = NewBotData();
        var dictionary = new Dictionary<string, string>();

        DictionaryMethods.AddKeyValuePair(data, dictionary, "alpha", "1");
        DictionaryMethods.RemoveByKey(data, dictionary, "alpha");

        Assert.Empty(dictionary);
    }

    [Fact]
    public void CheckCondition_StringComparison_ReturnsTrueForContains()
    {
        var data = NewBotData();

        var result = ConditionsMethods.CheckCondition(data, "alphabet", StrComparison.Contains, "pha");

        Assert.True(result);
    }

    [Fact]
    public void CheckCondition_ListComparison_ReturnsTrueForExactElement()
    {
        var data = NewBotData();

        var result = ConditionsMethods.CheckCondition(data, ["alpha", "beta"], ListComparison.Contains, "beta");

        Assert.True(result);
    }

    [Fact]
    public void CheckCondition_DictionaryComparison_ReturnsTrueForContainsValue()
    {
        var data = NewBotData();

        var result = ConditionsMethods.CheckCondition(
            data,
            new Dictionary<string, string> { ["alpha"] = "one" },
            DictComparison.HasValue,
            "one");

        Assert.True(result);
    }

    [Fact]
    public void CheckGlobalBanKeys_ReturnsTrueWhenProviderMatches()
    {
        var data = NewBotData(new MatchingProxySettingsProvider("BAN", string.Empty));
        data.SOURCE = "prefix BAN suffix";

        var result = ConditionsMethods.CheckGlobalBanKeys(data);

        Assert.True(result);
    }

    [Fact]
    public void CheckGlobalRetryKeys_ReturnsTrueWhenProviderMatches()
    {
        var data = NewBotData(new MatchingProxySettingsProvider(string.Empty, "RETRY"));
        data.SOURCE = "prefix RETRY suffix";

        var result = ConditionsMethods.CheckGlobalRetryKeys(data);

        Assert.True(result);
    }

    private static BotData NewBotData(IProxySettingsProvider? proxySettingsProvider = null)
        => new(
            new BotProviders(null!)
            {
                ProxySettings = proxySettingsProvider ?? new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));

    private sealed class MatchingProxySettingsProvider(string banKey, string retryKey) : IProxySettingsProvider
    {
        public TimeSpan ConnectTimeout => TimeSpan.FromSeconds(10);

        public TimeSpan ReadWriteTimeout => TimeSpan.FromSeconds(10);

        public bool ContainsBanKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            var found = !string.IsNullOrEmpty(banKey) && text.Contains(banKey);
            matchedKey = found ? banKey : string.Empty;
            return found;
        }

        public bool ContainsRetryKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            var found = !string.IsNullOrEmpty(retryKey) && text.Contains(retryKey);
            matchedKey = found ? retryKey : string.Empty;
            return found;
        }
    }
}
