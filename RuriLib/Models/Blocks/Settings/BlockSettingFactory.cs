using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// Creates <see cref="BlockSetting"/> instances for the supported parameter types.
/// </summary>
public static class BlockSettingFactory
{
    /// <summary>
    /// Creates a boolean block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateBoolSetting(string name, bool defaultValue = false,
        SettingInputMode mode = SettingInputMode.Fixed, string? defaultVariableName = null,
        string? readableName = null, string? description = null)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InputVariableName = defaultVariableName ?? string.Empty,
            FixedSetting = new BoolSetting { Value = defaultValue }
        };

    /// <summary>
    /// Creates an integer block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <param name="useLong">Whether generated code should pass this setting as a long.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateIntSetting(string name, long defaultValue = 0,
        SettingInputMode mode = SettingInputMode.Fixed, string? defaultVariableName = null,
        string? readableName = null, string? description = null, bool useLong = true)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InputVariableName = defaultVariableName ?? string.Empty,
            FixedSetting = new IntSetting { Value = defaultValue, UseLong = useLong }
        };

    /// <summary>
    /// Creates a float block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <param name="useDouble">Whether generated code should pass this setting as a double.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateFloatSetting(string name, double defaultValue = 0,
        SettingInputMode mode = SettingInputMode.Fixed, string? defaultVariableName = null,
        string? readableName = null, string? description = null, bool useDouble = true)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InputVariableName = defaultVariableName ?? string.Empty,
            FixedSetting = new FloatSetting { Value = defaultValue, UseDouble = useDouble }
        };

    /// <summary>
    /// Creates a byte array block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateByteArraySetting(string name, byte[]? defaultValue = null,
        SettingInputMode mode = SettingInputMode.Fixed, string? defaultVariableName = null,
        string? readableName = null, string? description = null)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InputVariableName = defaultVariableName ?? string.Empty,
            FixedSetting = new ByteArraySetting { Value = defaultValue ?? [] }
        };

    /// <summary>
    /// Creates an enum block setting for the given enum type parameter.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default enum value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateEnumSetting<T>(string name, string defaultValue = "",
        SettingInputMode mode = SettingInputMode.Fixed, string? defaultVariableName = null,
        string? readableName = null, string? description = null)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InputVariableName = defaultVariableName ?? string.Empty,
            FixedSetting = new EnumSetting(typeof(T)) { Value = defaultValue }
        };

    /// <summary>
    /// Creates an enum block setting for the provided runtime enum type.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="enumType">The enum type.</param>
    /// <param name="defaultValue">The default enum value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateEnumSetting(string name, Type enumType, string defaultValue = "",
        SettingInputMode mode = SettingInputMode.Fixed, string? defaultVariableName = null,
        string? readableName = null, string? description = null)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InputVariableName = defaultVariableName ?? string.Empty,
            FixedSetting = new EnumSetting(enumType) { Value = defaultValue }
        };

    /// <summary>
    /// Creates a string block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default value or variable name, depending on mode.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="multiLine">Whether the editor should allow multiple lines.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateStringSetting(string name, string? defaultValue = "",
        SettingInputMode mode = SettingInputMode.Fixed, bool multiLine = false,
        string? readableName = null, string? description = null)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InputVariableName = defaultValue ?? string.Empty,
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

    /// <summary>
    /// Creates a list-of-strings block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateListOfStringsSetting(string name, List<string>? defaultValue = null,
        SettingInputMode mode = SettingInputMode.Fixed, string? readableName = null,
        string? description = null)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InterpolatedSetting = new InterpolatedListOfStringsSetting
            {
                Value = defaultValue ?? []
            },
            FixedSetting = new ListOfStringsSetting
            {
                Value = defaultValue ?? []
            }
        };

    /// <summary>
    /// Creates a variable-backed list-of-strings block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="variableName">The source variable name.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateListOfStringsSetting(string name, string variableName,
        string? readableName = null, string? description = null)
    {
        var setting = CreateListOfStringsSetting(name, null, SettingInputMode.Variable,
            readableName, description);
        setting.InputVariableName = variableName;
        return setting;
    }

    /// <summary>
    /// Creates a dictionary-of-strings block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateDictionaryOfStringsSetting(string name,
        Dictionary<string, string>? defaultValue = null,
        SettingInputMode mode = SettingInputMode.Fixed, string? readableName = null,
        string? description = null)
        => new()
        {
            Name = name,
            Description = description,
            ReadableName = readableName ?? name.ToReadableName(),
            InputMode = mode,
            InterpolatedSetting = new InterpolatedDictionaryOfStringsSetting
            {
                Value = defaultValue ?? []
            },
            FixedSetting = new DictionaryOfStringsSetting
            {
                Value = defaultValue ?? []
            }
        };

    /// <summary>
    /// Creates a variable-backed dictionary-of-strings block setting.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="variableName">The source variable name.</param>
    /// <param name="readableName">The optional display name.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The created setting.</returns>
    public static BlockSetting CreateDictionaryOfStringsSetting(string name, string variableName,
        string? readableName = null, string? description = null)
    {
        var setting = CreateDictionaryOfStringsSetting(name, null, SettingInputMode.Variable,
            readableName, description);
        setting.InputVariableName = variableName;
        return setting;
    }
}
