using System;
using System.Collections.Generic;
using RuriLib.Exceptions;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Configs;
using Xunit;

namespace RuriLib.Tests.Models.Blocks.Custom;

public class KeycheckBlockInstanceTests
{
    private readonly string _nl = Environment.NewLine;

    /*
    [Fact]
    public void ToLC_NormalBlock_OutputScript()
    {
        var repo = new DescriptorsRepository();
        var descriptor = repo.GetAs<KeycheckBlockDescriptor>("Keycheck");
        var block = new KeycheckBlockInstance(descriptor);

        var banIfNoMatch = block.Settings["banIfNoMatch"];
        banIfNoMatch.InputMode = SettingInputMode.Fixed;
        (banIfNoMatch.FixedSetting as BoolSetting).Value = false;

        block.Disabled = true;
        block.Label = "My Label";

        block.Keychains = new List<Keychain>
        {
            new Keychain
            {
                ResultStatus = "SUCCESS",
                Mode = KeychainMode.OR,
                Keys = new List<Key>
                {
                    new StringKey
                    {
                        Left = new BlockSetting
                        {
                            InputMode = SettingInputMode.Variable,
                            InputVariableName = "myString"
                        },
                        Comparison = StrComparison.Contains,
                        Right = new BlockSetting
                        {
                            FixedSetting = new StringSetting { Value = "abc" }
                        }
                    },
                    new FloatKey
                    {
                        Left = new BlockSetting
                        {
                            FixedSetting = new FloatSetting { Value = 3f }
                        },
                        Comparison = NumComparison.GreaterThan,
                        Right = new BlockSetting
                        {
                            FixedSetting = new FloatSetting { Value = 1.5f }
                        }
                    }
                }
            },
            new Keychain
            {
                ResultStatus = "FAIL",
                Mode = KeychainMode.AND,
                Keys = new List<Key>
                {
                    new ListKey
                    {
                        Left = new BlockSetting
                        {
                            InputMode = SettingInputMode.Variable,
                            InputVariableName = "myList"
                        },
                        Comparison = ListComparison.Contains,
                        Right = new BlockSetting
                        {
                            FixedSetting = new StringSetting { Value = "abc" }
                        }
                    },
                    new DictionaryKey
                    {
                        Left = new BlockSetting
                        {
                            InputMode = SettingInputMode.Variable,
                            InputVariableName = "myDict"
                        },
                        Comparison = DictComparison.HasKey,
                        Right = new BlockSetting
                        {
                            FixedSetting = new StringSetting { Value = "abc" }
                        }
                    }
                }
            }
        };

        var expected = "DISABLED\r\nLABEL:My Label\r\n  banIfNoMatch = False\r\n  KEYCHAIN SUCCESS OR\r\n    STRINGKEY @myString Contains \"abc\"\r\n    FLOATKEY 3 GreaterThan 1.5\r\n  KEYCHAIN FAIL AND\r\n    LISTKEY @myList Contains \"abc\"\r\n    DICTKEY @myDict HasKey \"abc\"\r\n";
        Assert.Equal(expected, block.ToLC());
    }
    */

    [Fact]
    public void ToLC_NormalBlock_OutputScript()
    {
        var block = CreateBlock();

        var banIfNoMatch = block.Settings["banIfNoMatch"];
        banIfNoMatch.InputMode = SettingInputMode.Fixed;
        (banIfNoMatch.FixedSetting as BoolSetting)!.Value = false;

        block.Disabled = true;
        block.Label = "My Label";
        block.Keychains = CreateKeychains();

        var expected = $"DISABLED{_nl}LABEL:My Label{_nl}  banIfNoMatch = False{_nl}  KEYCHAIN SUCCESS OR{_nl}    STRINGKEY @myString Contains \"abc\"{_nl}    FLOATKEY 3 GreaterThan 1.5{_nl}  KEYCHAIN FAIL AND{_nl}    LISTKEY @myList Contains \"abc\"{_nl}    DICTKEY @myDict HasKey \"abc\"{_nl}";
        Assert.Equal(expected, block.ToLC());
    }

    [Fact]
    public void FromLC_NormalScript_BuildBlock()
    {
        var block = BlockFactory.GetBlock<KeycheckBlockInstance>("Keycheck");
        var script = $"DISABLED{_nl}LABEL:My Label{_nl}  banIfNoMatch = False{_nl}  KEYCHAIN SUCCESS OR{_nl}    STRINGKEY @myString Contains \"abc\"{_nl}    FLOATKEY 3 GreaterThan 1.5{_nl}  KEYCHAIN FAIL AND{_nl}    LISTKEY @myList Contains \"abc\"{_nl}    DICTKEY @myDict HasKey \"abc\"{_nl}";
        var lineNumber = 0;

        block.FromLC(ref script, ref lineNumber);

        Assert.True(block.Disabled);
        Assert.Equal("My Label", block.Label);

        var banIfNoMatch = block.Settings["banIfNoMatch"];
        Assert.False((banIfNoMatch.FixedSetting as BoolSetting)!.Value);

        var firstKeychain = block.Keychains[0];
        Assert.Equal("SUCCESS", firstKeychain.ResultStatus);
        Assert.Equal(KeychainMode.OR, firstKeychain.Mode);

        var firstKey = (StringKey)firstKeychain.Keys[0];
        Assert.Equal(SettingInputMode.Variable, firstKey.Left.InputMode);
        Assert.Equal("myString", firstKey.Left.InputVariableName);
        Assert.Equal(StrComparison.Contains, firstKey.Comparison);
        Assert.Equal("abc", (firstKey.Right.FixedSetting as StringSetting)!.Value);

        var secondKey = (FloatKey)firstKeychain.Keys[1];
        Assert.Equal(3F, (secondKey.Left.FixedSetting as FloatSetting)!.Value);
        Assert.Equal(NumComparison.GreaterThan, secondKey.Comparison);
        Assert.Equal(1.5F, (secondKey.Right.FixedSetting as FloatSetting)!.Value);

        var secondKeychain = block.Keychains[1];
        Assert.Equal("FAIL", secondKeychain.ResultStatus);
        Assert.Equal(KeychainMode.AND, secondKeychain.Mode);

        var thirdKey = (ListKey)secondKeychain.Keys[0];
        Assert.Equal("myList", thirdKey.Left.InputVariableName);
        Assert.Equal(ListComparison.Contains, thirdKey.Comparison);
        Assert.Equal("abc", (thirdKey.Right.FixedSetting as StringSetting)!.Value);

        var fourthKey = (DictionaryKey)secondKeychain.Keys[1];
        Assert.Equal("myDict", fourthKey.Left.InputVariableName);
        Assert.Equal(DictComparison.HasKey, fourthKey.Comparison);
        Assert.Equal("abc", (fourthKey.Right.FixedSetting as StringSetting)!.Value);
    }

    [Fact]
    public void FromLC_KeyWithoutKeychain_Throws()
    {
        var block = BlockFactory.GetBlock<KeycheckBlockInstance>("Keycheck");
        var script = $"    STRINGKEY @myString Contains \"abc\"{_nl}";
        var lineNumber = 0;

        Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));
    }

    [Fact]
    public void FromLC_InvalidKeyDeclaration_PreservesLineParsingDetails()
    {
        var block = BlockFactory.GetBlock<KeycheckBlockInstance>("Keycheck");
        var script = $"  KEYCHAIN SUCCESS OR{_nl}    STRINGKEY @myString Contains{_nl}";
        var lineNumber = 0;

        var ex = Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));

        Assert.Equal(2, ex.LineNumber);
        Assert.Equal(1, ex.ColumnNumber);
        Assert.IsType<LineParsingException>(ex.InnerException);
        Assert.Contains("Expected '\"' to start a string literal", ex.Message);
    }

    [Fact]
    public void ToSyntax_NormalBlock_OutputScript()
    {
        var block = CreateBlock();

        var banIfNoMatch = block.Settings["banIfNoMatch"];
        banIfNoMatch.InputMode = SettingInputMode.Variable;
        banIfNoMatch.InputVariableName = "myBool";

        block.Disabled = true;
        block.Label = "My Label";
        block.Keychains = CreateKeychains();

        var expected = $"data.Logger.LogHeader(\"CheckCondition\");{_nl}if (CheckCondition(data, myString.AsString(), StrComparison.Contains, \"abc\") || CheckCondition(data, 3F, NumComparison.GreaterThan, 1.5F)){_nl} {{ data.STATUS = \"SUCCESS\"; }}{_nl}else if (CheckCondition(data, myList.AsList(), ListComparison.Contains, \"abc\") && CheckCondition(data, myDict.AsDict(), DictComparison.HasKey, \"abc\")){_nl}  {{ data.STATUS = \"FAIL\"; return; }}{_nl}else if (myBool.AsBool()){_nl}  {{ data.STATUS = \"BAN\"; return; }}{_nl}if (CheckGlobalBanKeys(data)) {{ data.STATUS = \"BAN\"; return; }}{_nl}if (CheckGlobalRetryKeys(data)) {{ data.STATUS = \"RETRY\"; return; }}{_nl}";
        Assert.Equal(NormalizeSnippet(expected), RenderSyntax(block, new ConfigSettings()));
    }

    [Fact]
    public void ToSyntax_NoKeychains_OutputScript()
    {
        var block = CreateBlock();

        var banIfNoMatch = block.Settings["banIfNoMatch"];
        banIfNoMatch.InputMode = SettingInputMode.Variable;
        banIfNoMatch.InputVariableName = "myBool";

        var expected = $"data.Logger.LogHeader(\"CheckCondition\");{_nl}if (myBool.AsBool()){_nl}  {{ data.STATUS = \"BAN\"; return; }}{_nl}if (CheckGlobalBanKeys(data)) {{ data.STATUS = \"BAN\"; return; }}{_nl}if (CheckGlobalRetryKeys(data)) {{ data.STATUS = \"RETRY\"; return; }}{_nl}";
        Assert.Equal(NormalizeSnippet(expected), RenderSyntax(block, new ConfigSettings()));
    }

    [Fact]
    public void ToSyntax_TracksExpectedStatusFlowVariants()
    {
        AssertSyntax(CreateVariableBanBlock(), new ConfigSettings(),
            "data.Logger.LogHeader(\"CheckCondition\")",
            "if (CheckCondition(data, ObjectExtensions.DynamicAsString(globals.myString), StrComparison.Contains, \"abc\") || CheckCondition(data, 3F, NumComparison.GreaterThan, 1.5F))",
            "data.STATUS = \"SUCCESS\";",
            "else if (CheckCondition(data, ObjectExtensions.DynamicAsList(globals.myList), ListComparison.Contains, \"abc\") && CheckCondition(data, ObjectExtensions.DynamicAsDict(input.myDict), DictComparison.HasKey, \"abc\"))",
            "data.STATUS = \"FAIL\";",
            "else if (ObjectExtensions.DynamicAsBool(globals.shouldBan))",
            "data.STATUS = \"BAN\";",
            "if (CheckGlobalBanKeys(data))",
            "if (CheckGlobalRetryKeys(data))");

        AssertSyntax(CreateFixedBanBlock(true), new ConfigSettings
        {
            GeneralSettings = { ContinueStatuses = ["SUCCESS", "NONE", "BAN"] }
        },
            "data.STATUS = \"SUCCESS\";",
            "data.STATUS = \"FAIL\";",
            "if (CheckGlobalRetryKeys(data))",
            "data.STATUS = \"RETRY\";");

        AssertSyntax(CreateFixedBanBlock(false), new ConfigSettings
        {
            GeneralSettings = { ContinueStatuses = ["SUCCESS", "NONE", "FAIL", "RETRY"] }
        },
            "data.STATUS = \"SUCCESS\";",
            "if (CheckGlobalBanKeys(data))",
            "data.STATUS = \"BAN\";");

        AssertSyntax(CreateNoKeychainsBlock(), new ConfigSettings
        {
            GeneralSettings = { ContinueStatuses = ["SUCCESS", "NONE", "BAN", "RETRY"] }
        },
            "if (myBool.AsBool())",
            "data.STATUS = \"BAN\";",
            "if (CheckGlobalRetryKeys(data))",
            "data.STATUS = \"RETRY\";");
    }

    private static KeycheckBlockInstance CreateBlock()
        => new(new KeycheckBlockDescriptor());

    private static KeycheckBlockInstance CreateVariableBanBlock()
    {
        var block = CreateBlock();
        block.Settings["banIfNoMatch"].InputMode = SettingInputMode.Variable;
        block.Settings["banIfNoMatch"].InputVariableName = "globals.shouldBan";
        block.Keychains = CreateParityKeychains();
        return block;
    }

    private static KeycheckBlockInstance CreateFixedBanBlock(bool value)
    {
        var block = CreateBlock();
        block.Settings["banIfNoMatch"].InputMode = SettingInputMode.Fixed;
        (block.Settings["banIfNoMatch"].FixedSetting as BoolSetting)!.Value = value;
        block.Keychains = CreateParityKeychains();
        return block;
    }

    private static KeycheckBlockInstance CreateNoKeychainsBlock()
    {
        var block = CreateBlock();
        block.Settings["banIfNoMatch"].InputMode = SettingInputMode.Variable;
        block.Settings["banIfNoMatch"].InputVariableName = "myBool";
        return block;
    }

    private static List<Keychain> CreateKeychains()
        => new()
        {
            new Keychain
            {
                ResultStatus = "SUCCESS",
                Mode = KeychainMode.OR,
                Keys =
                [
                    new StringKey
                    {
                        Left = BlockSettingFactory.CreateStringSetting("", "myString", SettingInputMode.Variable),
                        Comparison = StrComparison.Contains,
                        Right = BlockSettingFactory.CreateStringSetting("", "abc", SettingInputMode.Fixed)
                    },
                    new FloatKey
                    {
                        Left = BlockSettingFactory.CreateFloatSetting("", 3F),
                        Comparison = NumComparison.GreaterThan,
                        Right = BlockSettingFactory.CreateFloatSetting("", 1.5F)
                    }
                ]
            },
            new Keychain
            {
                ResultStatus = "FAIL",
                Mode = KeychainMode.AND,
                Keys =
                [
                    new ListKey
                    {
                        Left = BlockSettingFactory.CreateListOfStringsSetting("", "myList"),
                        Comparison = ListComparison.Contains,
                        Right = BlockSettingFactory.CreateStringSetting("", "abc", SettingInputMode.Fixed)
                    },
                    new DictionaryKey
                    {
                        Left = BlockSettingFactory.CreateDictionaryOfStringsSetting("", "myDict"),
                        Comparison = DictComparison.HasKey,
                        Right = BlockSettingFactory.CreateStringSetting("", "abc", SettingInputMode.Fixed)
                    }
                ]
            }
        };

    private static List<Keychain> CreateParityKeychains()
        => new()
        {
            new Keychain
            {
                ResultStatus = "SUCCESS",
                Mode = KeychainMode.OR,
                Keys =
                [
                    new StringKey
                    {
                        Left = BlockSettingFactory.CreateStringSetting("", "globals.myString", SettingInputMode.Variable),
                        Comparison = StrComparison.Contains,
                        Right = BlockSettingFactory.CreateStringSetting("", "abc", SettingInputMode.Fixed)
                    },
                    new FloatKey
                    {
                        Left = BlockSettingFactory.CreateFloatSetting("", 3F),
                        Comparison = NumComparison.GreaterThan,
                        Right = BlockSettingFactory.CreateFloatSetting("", 1.5F)
                    }
                ]
            },
            new Keychain
            {
                ResultStatus = "FAIL",
                Mode = KeychainMode.AND,
                Keys =
                [
                    new ListKey
                    {
                        Left = BlockSettingFactory.CreateListOfStringsSetting("", "globals.myList"),
                        Comparison = ListComparison.Contains,
                        Right = BlockSettingFactory.CreateStringSetting("", "abc", SettingInputMode.Fixed)
                    },
                    new DictionaryKey
                    {
                        Left = BlockSettingFactory.CreateDictionaryOfStringsSetting("", "input.myDict"),
                        Comparison = DictComparison.HasKey,
                        Right = BlockSettingFactory.CreateStringSetting("", "abc", SettingInputMode.Fixed)
                    }
                ]
            }
        };

    private static void AssertSyntax(KeycheckBlockInstance block, ConfigSettings settings, params string[] expectedFragments)
    {
        var syntax = block.ToSyntax(new BlockSyntaxGenerationContext([], settings)).ToSnippet();

        foreach (var expectedFragment in expectedFragments)
        {
            Assert.Contains(expectedFragment, syntax);
        }
    }

    private static string RenderSyntax(KeycheckBlockInstance block, ConfigSettings settings)
        => block.ToSyntax(new BlockSyntaxGenerationContext([], settings)).ToSnippet();

    private static string NormalizeSnippet(string snippet)
        => StatementSyntaxParser.ParseStatements(snippet).ToSnippet();
}
