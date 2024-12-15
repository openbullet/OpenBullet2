using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of type list of strings.
/// </summary>
public class ListOfStringsParameter : BlockParameter
{
    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public List<string> DefaultValue { get; set; } = [];

    /// <summary></summary>
    public ListOfStringsParameter(string name) : base(name)
    {

    }

    /// <summary></summary>
    public ListOfStringsParameter(string name, List<string>? defaultValue = null,
        SettingInputMode inputMode = SettingInputMode.Fixed) : base(name)
    {
        DefaultValue = defaultValue ?? [];
        DefaultVariableName = string.Empty;
        InputMode = inputMode;
    }

    /// <summary></summary>
    public ListOfStringsParameter(string name, string defaultVariableName = "")
        : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => BlockSettingFactory.CreateListOfStringsSetting(Name, DefaultValue, InputMode, PrettyName, Description);
}
