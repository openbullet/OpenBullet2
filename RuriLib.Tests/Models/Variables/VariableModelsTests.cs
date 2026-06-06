using RuriLib.Models.Captchas;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using Xunit;

namespace RuriLib.Tests.Models.Variables;

public class VariableModelsTests
{
    [Fact]
    public void ByteArrayVariable_NullValue_RendersNullString()
    {
        var variable = new ByteArrayVariable(null);

        Assert.Equal("null", variable.AsString());
        Assert.Null(variable.AsByteArray());
    }

    [Fact]
    public void ListOfStringsVariable_NullValue_RendersNullString()
    {
        var variable = new ListOfStringsVariable(null);

        Assert.Equal("null", variable.AsString());
        Assert.Null(variable.AsListOfStrings());
    }

    [Fact]
    public void DictionaryOfStringsVariable_NullValue_RendersNullString()
    {
        var variable = new DictionaryOfStringsVariable(null);

        Assert.Equal("null", variable.AsString());
        Assert.Null(variable.AsDictionaryOfStrings());
    }

    [Fact]
    public void VariableFactory_NullObject_ThrowsWithHelpfulType()
    {
        var exception = Assert.Throws<NotSupportedException>(() => VariableFactory.FromObject(null!));

        Assert.Contains("null", exception.Message);
    }

    [Fact]
    public void CaptchaInfo_Defaults_AreSafe()
    {
        var info = new CaptchaInfo();

        Assert.Equal(string.Empty, info.Id);
    }

    [Fact]
    public void StringVariable_AsFloat_UsesInvariantCulture()
    {
        var variable = new StringVariable("1.5");

        Assert.Equal(1.5f, variable.AsFloat());
    }

    [Fact]
    public void IntVariable_StoresLongAndKeepsLegacyIntCast()
    {
        var variable = new IntVariable(5_000_000_000L);

        Assert.Equal(5_000_000_000L, variable.AsLong());
        Assert.Throws<OverflowException>(() => variable.AsInt());
    }

    [Fact]
    public void FloatVariable_StoresDoubleAndKeepsLegacyFloatCast()
    {
        var variable = new FloatVariable(3.141592653589793D);

        Assert.Equal(3.141592653589793D, variable.AsDouble());
        Assert.Equal((float)3.141592653589793D, variable.AsFloat());
    }

    [Fact]
    public void DictionaryOfStringsVariable_AsString_FormatsEntries()
    {
        var variable = new DictionaryOfStringsVariable(new Dictionary<string, string>
        {
            ["a"] = "b"
        });

        Assert.Equal("{(a, b)}", variable.AsString());
    }
}
