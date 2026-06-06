using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// A setting that holds an enum value.
/// </summary>
public class EnumSetting : Setting
{
    private Type _enumType = null!;
    private readonly Dictionary<string, string> _enumValues = [];
    private readonly Dictionary<string, string> _prettyNamesByValue = [];

    /// <summary>
    /// Creates a new enum setting for the given enum type.
    /// </summary>
    /// <param name="enumType">The enum type represented by this setting.</param>
    public EnumSetting(Type enumType)
    {
        EnumType = enumType;
    }

    /// <summary>
    /// The type of the enum.
    /// </summary>
    public Type EnumType
    {
        get => _enumType;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!value.IsEnum)
            {
                throw new ArgumentException($"{value} is not an enum type", nameof(value));
            }

            _enumType = value;
            _enumValues.Clear();
            _prettyNamesByValue.Clear();

            foreach (var name in _enumType.GetEnumNames())
            {
                var enumField = _enumType.GetField(name);
                var prettyName = name;

                if (enumField?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    is DescriptionAttribute[] attributes && attributes.Length > 0)
                {
                    prettyName = attributes[0].Description;
                }

                _enumValues[prettyName] = name;
                _prettyNamesByValue[name] = prettyName;
            }
        }
    }

    /// <summary>
    /// The value of the setting.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The pretty names of the enum values.
    /// </summary>
    public IEnumerable<string> PrettyNames => _enumValues.Keys;

    /// <summary>
    /// The pretty name of the current value.
    /// </summary>
    public string PrettyName
        => _prettyNamesByValue.TryGetValue(Value, out var prettyName)
            ? prettyName
            : Value;

    /// <summary>
    /// Sets the value of the setting from a pretty name.
    /// </summary>
    /// <param name="prettyName">The display name of the enum member.</param>
    public void SetFromPrettyName(string prettyName)
    {
        if (!_enumValues.TryGetValue(prettyName, out var value))
        {
            throw new KeyNotFoundException($"The enum pretty name {prettyName} was not found");
        }

        Value = value;
    }
}
