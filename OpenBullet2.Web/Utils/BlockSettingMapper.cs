using OpenBullet2.Web.Dtos.Config.Blocks;
using OpenBullet2.Web.Exceptions;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System.Text.Json;

namespace OpenBullet2.Web.Utils;

internal static class BlockSettingMapper
{
    internal static BlockSettingDto ToDto(BlockSetting setting) => new()
    {
        Name = setting.Name,
        Value = GetValue(setting),
        InputMode = setting.InputMode,
        InputVariableName = setting.InputVariableName,
        Type = GetType(setting)
    };

    internal static void Apply(BlockSettingDto? dto, BlockSetting setting)
    {
        if (dto is null)
        {
            return;
        }

        setting.InputMode = dto.InputMode;
        setting.InputVariableName = dto.InputVariableName ?? string.Empty;
        var value = (JsonElement)dto.Value!;

        switch (dto.Type)
        {
            case BlockSettingType.String:
                ((StringSetting)setting.FixedSetting!).Value = value.GetString();
                ((InterpolatedStringSetting)setting.InterpolatedSetting!).Value = value.GetString();
                break;

            case BlockSettingType.Int:
                ((IntSetting)setting.FixedSetting!).Value = value.GetInt64();
                break;

            case BlockSettingType.Float:
                ((FloatSetting)setting.FixedSetting!).Value = value.GetDouble();
                break;

            case BlockSettingType.Bool:
                ((BoolSetting)setting.FixedSetting!).Value = value.GetBoolean();
                break;

            case BlockSettingType.ByteArray:
                ((ByteArraySetting)setting.FixedSetting!).Value = value.GetBytesFromBase64();
                break;

            case BlockSettingType.ListOfStrings:
                ((ListOfStringsSetting)setting.FixedSetting!).Value = value
                    .Deserialize<List<string>>(Globals.JsonOptions)!;
                ((InterpolatedListOfStringsSetting)setting.InterpolatedSetting!).Value = value
                    .Deserialize<List<string>>(Globals.JsonOptions)!;
                break;

            case BlockSettingType.DictionaryOfStrings:
                ((DictionaryOfStringsSetting)setting.FixedSetting!).Value = value
                    .Deserialize<Dictionary<string, string>>(Globals.JsonOptions)!;
                ((InterpolatedDictionaryOfStringsSetting)setting.InterpolatedSetting!).Value = value
                    .Deserialize<Dictionary<string, string>>(Globals.JsonOptions)!;
                break;

            case BlockSettingType.Enum:
                ((EnumSetting)setting.FixedSetting!).Value = value.GetString() ?? string.Empty;
                break;

            default:
                throw new MappingException($"Unsupported block setting type {dto.Type}");
        }
    }

    private static object GetValue(BlockSetting setting)
    {
        if (setting.InputMode is SettingInputMode.Interpolated)
        {
            return setting.InterpolatedSetting switch
            {
                InterpolatedStringSetting x => x.Value ?? string.Empty,
                InterpolatedListOfStringsSetting x => x.Value ?? [],
                InterpolatedDictionaryOfStringsSetting x => x.Value ?? [],
                _ => throw new NotImplementedException()
            };
        }

        return setting.FixedSetting switch
        {
            StringSetting x => x.Value ?? string.Empty,
            IntSetting x => x.Value,
            FloatSetting x => x.Value,
            BoolSetting x => x.Value,
            ByteArraySetting x => x.Value ?? [],
            EnumSetting x => x.Value,
            ListOfStringsSetting x => x.Value ?? [],
            DictionaryOfStringsSetting x => x.Value ?? [],
            _ => throw new NotImplementedException()
        };
    }

    private static BlockSettingType GetType(BlockSetting setting)
    {
        if (setting.InputMode is SettingInputMode.Interpolated)
        {
            return setting.InterpolatedSetting switch
            {
                InterpolatedStringSetting => BlockSettingType.String,
                InterpolatedListOfStringsSetting => BlockSettingType.ListOfStrings,
                InterpolatedDictionaryOfStringsSetting => BlockSettingType.DictionaryOfStrings,
                _ => throw new NotImplementedException()
            };
        }

        return setting.FixedSetting switch
        {
            StringSetting => BlockSettingType.String,
            IntSetting => BlockSettingType.Int,
            FloatSetting => BlockSettingType.Float,
            BoolSetting => BlockSettingType.Bool,
            ByteArraySetting => BlockSettingType.ByteArray,
            EnumSetting => BlockSettingType.Enum,
            ListOfStringsSetting => BlockSettingType.ListOfStrings,
            DictionaryOfStringsSetting => BlockSettingType.DictionaryOfStrings,
            _ => throw new NotImplementedException()
        };
    }
}
