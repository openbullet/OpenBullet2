using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Models.Blocks.Custom
{
    public class KeycheckBlockInstanceTests
    {
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

        [Fact]
        public void FromLC_NormalScript_BuildBlock()
        {
            var block = BlockFactory.GetBlock<KeycheckBlockInstance>("Keycheck");
            var script = "DISABLED\r\nLABEL:My Label\r\n  banIfNoMatch = False\r\n  KEYCHAIN SUCCESS OR\r\n    STRINGKEY @myString Contains \"abc\"\r\n    FLOATKEY 3 GreaterThan 1.5\r\n  KEYCHAIN FAIL AND\r\n    LISTKEY @myList Contains \"abc\"\r\n    DICTKEY @myDict HasKey \"abc\"\r\n";
            int lineNumber = 0;
            block.FromLC(ref script, ref lineNumber);

            Assert.True(block.Disabled);
            Assert.Equal("My Label", block.Label);

            BlockSetting left, right;

            var banIfNoMatch = block.Settings["banIfNoMatch"];
            Assert.False((banIfNoMatch.FixedSetting as BoolSetting).Value);

            var kc1 = block.Keychains[0];

            Assert.Equal("SUCCESS", kc1.ResultStatus);
            Assert.Equal(KeychainMode.OR, kc1.Mode);

            var k1 = kc1.Keys[0] as StringKey;
            var k2 = kc1.Keys[1] as FloatKey;

            left = k1.Left;
            right = k1.Right;
            Assert.Equal(SettingInputMode.Variable, left.InputMode);
            Assert.Equal("myString", left.InputVariableName);
            Assert.Equal(StrComparison.Contains, k1.Comparison);
            Assert.Equal(SettingInputMode.Fixed, right.InputMode);
            Assert.Equal("abc", (right.FixedSetting as StringSetting).Value);

            left = k2.Left;
            right = k2.Right;
            Assert.Equal(SettingInputMode.Fixed, left.InputMode);
            Assert.Equal(3F, (left.FixedSetting as FloatSetting).Value);
            Assert.Equal(NumComparison.GreaterThan, k2.Comparison);
            Assert.Equal(SettingInputMode.Fixed, right.InputMode);
            Assert.Equal(1.5F, (right.FixedSetting as FloatSetting).Value);

            var kc2 = block.Keychains[1];

            Assert.Equal("FAIL", kc2.ResultStatus);
            Assert.Equal(KeychainMode.AND, kc2.Mode);

            var k3 = kc2.Keys[0] as ListKey;
            var k4 = kc2.Keys[1] as DictionaryKey;

            left = k3.Left;
            right = k3.Right;
            Assert.Equal(SettingInputMode.Variable, left.InputMode);
            Assert.Equal("myList", left.InputVariableName);
            Assert.Equal(ListComparison.Contains, k3.Comparison);
            Assert.Equal(SettingInputMode.Fixed, right.InputMode);
            Assert.Equal("abc", (right.FixedSetting as StringSetting).Value);

            left = k4.Left;
            right = k4.Right;
            Assert.Equal(SettingInputMode.Variable, left.InputMode);
            Assert.Equal("myDict", left.InputVariableName);
            Assert.Equal(DictComparison.HasKey, k4.Comparison);
            Assert.Equal(SettingInputMode.Fixed, right.InputMode);
            Assert.Equal("abc", (right.FixedSetting as StringSetting).Value);
        }

        [Fact]
        public void ToCSharp_NormalBlock_OutputScript()
        {
            var repo = new DescriptorsRepository();
            var descriptor = repo.GetAs<KeycheckBlockDescriptor>("Keycheck");
            var block = new KeycheckBlockInstance(descriptor);

            var banIfNoMatch = block.Settings["banIfNoMatch"];
            banIfNoMatch.InputMode = SettingInputMode.Variable;
            banIfNoMatch.InputVariableName = "myBool";

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
                    }
                }
            };

            string expected = "if (Conditions.Check(myString.AsString(), StrComparison.Contains, \"abc\") || Conditions.Check(3F, NumComparison.GreaterThan, 1.5F))\r\n  data.STATUS = \"SUCCESS\";\r\nelse if (Conditions.Check(myList.AsList(), ListComparison.Contains, \"abc\") && Conditions.Check(myDict.AsDict(), DictComparison.HasKey, \"abc\"))\r\n  { data.STATUS = \"FAIL\"; return; }\r\nelse if (myBool.AsBool())\r\n  { data.STATUS = \"BAN\"; return; }\r\n";
            Assert.Equal(expected, block.ToCSharp(new List<string>(), new ConfigSettings()));
        }

        [Fact]
        public void ToCSharp_NoKeychains_OutputScript()
        {
            var repo = new DescriptorsRepository();
            var descriptor = repo.GetAs<KeycheckBlockDescriptor>("Keycheck");
            var block = new KeycheckBlockInstance(descriptor);

            var banIfNoMatch = block.Settings["banIfNoMatch"];
            banIfNoMatch.InputMode = SettingInputMode.Variable;
            banIfNoMatch.InputVariableName = "myBool";

            block.Disabled = true;
            block.Label = "My Label";

            block.Keychains = new List<Keychain> { };

            string expected = "if (myBool.AsBool())\r\n  { data.STATUS = \"BAN\"; return; }\r\n";
            Assert.Equal(expected, block.ToCSharp(new List<string>(), new ConfigSettings()));
        }
        */
    }
}
