using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings
{
    public static class BlockSettingFactory
    {
        public static BlockSetting CreateBoolSetting(string name, bool defaultValue = false, string readableName = null,
            string description = null)
            => new()
            {
                Name = name,
                Description = description,
                ReadableName = readableName ?? name.ToReadableName(),
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new BoolSetting { Value = defaultValue }
            };

        public static BlockSetting CreateIntSetting(string name, int defaultValue = 0, string readableName = null,
            string description = null)
            => new()
            {
                Name = name,
                Description = description,
                ReadableName = readableName ?? name.ToReadableName(),
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new IntSetting { Value = defaultValue }
            };

        public static BlockSetting CreateFloatSetting(string name, float defaultValue = 0, string readableName = null,
            string description = null)
            => new()
            {
                Name = name,
                Description = description,
                ReadableName = readableName ?? name.ToReadableName(),
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new FloatSetting { Value = defaultValue }
            };

        public static BlockSetting CreateByteArraySetting(string name, byte[] defaultValue = null, string readableName = null,
            string description = null)
            => new()
            {
                Name = name,
                Description= description,
                ReadableName = readableName ?? name.ToReadableName(),
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new ByteArraySetting { Value = defaultValue ?? new byte[] { } }
            };

        public static BlockSetting CreateEnumSetting<T>(string name, string defaultValue = "", string readableName = null,
            string description = null)
            => new()
            {
                Name = name,
                Description = description,
                ReadableName = readableName ?? name.ToReadableName(),
                InputMode = SettingInputMode.Fixed,
                FixedSetting = new EnumSetting { EnumType = typeof(T), Value = defaultValue }
            };

        public static BlockSetting CreateStringSetting(string name, string defaultValue = "",
            SettingInputMode mode = SettingInputMode.Fixed, bool multiLine = false, string readableName = null,
            string description = null) 
            => new()
            {
                Name = name,
                Description = description,
                ReadableName = readableName ?? name.ToReadableName(),
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

        public static BlockSetting CreateListOfStringsSetting(string name, List<string> defaultValue = null,
            SettingInputMode mode = SettingInputMode.Fixed, string readableName = null, string description = null)
            => new()
            {
                Name = name,
                Description = description,
                ReadableName = readableName ?? name.ToReadableName(),
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

        public static BlockSetting CreateListOfStringsSetting(string name, string variableName, string readableName = null,
            string description = null)
        {
            var setting = CreateListOfStringsSetting(name, null, SettingInputMode.Variable, readableName, description);
            setting.InputVariableName = variableName;
            return setting;
        }

        public static BlockSetting CreateDictionaryOfStringsSetting(string name, Dictionary<string, string> defaultValue = null,
            SettingInputMode mode = SettingInputMode.Fixed, string readableName = null, string description = null)
            => new()
            {
                Name = name,
                Description = description,
                ReadableName = readableName ?? name.ToReadableName(),
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

        public static BlockSetting CreateDictionaryOfStringsSetting(string name, string variableName, string readableName = null,
            string description = null)
        {
            var setting = CreateDictionaryOfStringsSetting(name, null, SettingInputMode.Variable, readableName, description);
            setting.InputVariableName = variableName;
            return setting;
        }
    }
}
