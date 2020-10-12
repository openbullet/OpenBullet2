using RuriLib.Models.Blocks.Settings.Interpolated;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings
{
    public static class BlockSettingFactory
    {
        public static BlockSetting CreateStringSetting(string name, string defaultValue = "", SettingInputMode mode = SettingInputMode.Fixed)
        {
            return new BlockSetting
            {
                Name = name,
                InputMode = mode,
                InterpolatedSetting = new InterpolatedStringSetting
                {
                    Value = defaultValue
                },
                FixedSetting = new StringSetting
                {
                    Value = defaultValue
                }
            };
        }

        public static BlockSetting CreateListOfStringsSetting(string name, List<string> defaultValue = null,
            SettingInputMode mode = SettingInputMode.Fixed)
        {
            return new BlockSetting
            {
                Name = name,
                InputMode = mode,
                InterpolatedSetting = new InterpolatedListOfStringsSetting
                {
                    Value = defaultValue ?? new List<string>()
                },
                FixedSetting = new ListOfStringsSetting
                {
                    Value = defaultValue ?? new List<string>()
                }
            };
        }

        public static BlockSetting CreateDictionaryOfStringsSetting(string name, Dictionary<string, string> defaultValue = null,
            SettingInputMode mode = SettingInputMode.Fixed)
        {
            return new BlockSetting
            {
                Name = name,
                InputMode = mode,
                InterpolatedSetting = new InterpolatedDictionaryOfStringsSetting
                {
                    Value = defaultValue ?? new Dictionary<string, string>()
                },
                FixedSetting = new DictionaryOfStringsSetting
                {
                    Value = defaultValue ?? new Dictionary<string, string>()
                }
            };
        }
    }
}
