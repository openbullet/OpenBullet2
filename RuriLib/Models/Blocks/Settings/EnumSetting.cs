using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// A setting that holds an enum value.
/// </summary>
public class EnumSetting : Setting
{
    private Type _enumType;
    private readonly Dictionary<string, string> _enumValues = [];

    public EnumSetting(Type enumType)
    {
        _enumType = enumType;
    }
    
    /// <summary>
    /// The type of the enum.
    /// </summary>
    public Type EnumType
    { 
        get => _enumType;
        set
        {
            _enumType = value;
            
            // Populate the enum values dictionary (used to have nicer enum names to display)
            foreach (var name in _enumType.GetEnumNames())
            {
                var fi = _enumType.GetField(name);

                if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
                {
                    _enumValues[attributes.First().Description] = name;
                }
                else
                {
                    _enumValues[name] = name;
                }
            }
        }
    }
    
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The pretty names of the enum values.
    /// </summary>
    public IEnumerable<string> PrettyNames => _enumValues.Keys;

    /// <summary>
    /// The pretty name of the current value.
    /// </summary>
    public string PrettyName
        => _enumValues.ContainsValue(Value)
            ? _enumValues.First(kvp => kvp.Value == Value).Key
            : Value;
    
    /// <summary>
    /// Sets the value of the setting from a pretty name.
    /// </summary>
    public void SetFromPrettyName(string prettyName) => Value = _enumValues[prettyName];
}
