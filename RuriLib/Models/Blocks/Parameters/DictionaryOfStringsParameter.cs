using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of type dictionary of strings.
/// </summary>
public class DictionaryOfStringsParameter : BlockParameter
{
    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public Dictionary<string, string> DefaultValue { get; set; } = [];

    /// <summary></summary>
    public DictionaryOfStringsParameter(string name) : base(name)
    {

    }

    /// <summary></summary>
    public DictionaryOfStringsParameter(string name, Dictionary<string, string>? defaultValue = null,
        SettingInputMode inputMode = SettingInputMode.Fixed) : base(name)
    {
        DefaultValue = defaultValue ?? [];
        DefaultVariableName = string.Empty;
        InputMode = inputMode;
    }

    /// <summary></summary>
    public DictionaryOfStringsParameter(string name, string defaultVariableName = "")
        : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => BlockSettingFactory.CreateDictionaryOfStringsSetting(Name, DefaultValue, InputMode, PrettyName, Description);
}
