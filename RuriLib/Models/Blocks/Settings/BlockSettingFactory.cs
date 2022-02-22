using RuriLib.Models.Blocks.Settings.Interpolated;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings
{
    public static class BlockSettingFactory
    {
        public static BlockSetting CreateBoolSetting(string name, bool defaultValue = false)
            => new BlockSetting
            {
                Name = name,
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new BoolSetting { Value = defaultValue }
            };

        public static BlockSetting CreateIntSetting(string name, int defaultValue = 0)
            => new BlockSetting
            {
                Name = name,
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new IntSetting { Value = defaultValue }
            };

        public static BlockSetting CreateFloatSetting(string name, float defaultValue = 0)
            => new BlockSetting
            {
                Name = name,
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new FloatSetting { Value = defaultValue }
            };

        public static BlockSetting CreateByteArraySetting(string name, byte[] defaultValue = null)
            => new BlockSetting
            {
                Name = name,
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new ByteArraySetting { Value = defaultValue ?? new byte[] { } }
            };

        public static BlockSetting CreateEnumSetting<T>(string name, string defaultValue = "")
            => new BlockSetting
            {
                Name = name,
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new EnumSetting(typeof(T)) { Value = defaultValue }
            };

        public static BlockSetting CreateStringSetting(string name, string defaultValue = "",
            SettingInputMode mode = SettingInputMode.Fixed, bool multiLine = false)
        {
            return new BlockSetting
            {
                Name = name,
                InputMode = mode,
                InputVariableName = defaultValue,
                InterpolatedSetting = new InterpolatedStringSetting
                {
                    Value = defaultValue,
                    MultiLine = multiLine
                },
                FixedSetting = new StringSetting
                {
                    Value = defaultValue,
                    MultiLine = multiLine
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

        public static BlockSetting CreateListOfStringsSetting(string name, string variableName)
        {
            var setting = CreateListOfStringsSetting(name, null, SettingInputMode.Variable);
            setting.InputVariableName = variableName;
            return setting;
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

        public static BlockSetting CreateDictionaryOfStringsSetting(string name, string variableName)
        {
            var setting = CreateDictionaryOfStringsSetting(name, null, SettingInputMode.Variable);
            setting.InputVariableName = variableName;
            return setting;
        }
    }
}
