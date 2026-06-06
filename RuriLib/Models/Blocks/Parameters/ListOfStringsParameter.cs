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

    /// <summary>
    /// Initializes a new instance of the <see cref="ListOfStringsParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    public ListOfStringsParameter(string name) : base(name)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListOfStringsParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="inputMode">The supported input mode.</param>
    public ListOfStringsParameter(string name, List<string>? defaultValue = null,
        SettingInputMode inputMode = SettingInputMode.Fixed) : base(name)
    {
        DefaultValue = defaultValue ?? [];
        DefaultVariableName = string.Empty;
        InputMode = inputMode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListOfStringsParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    public ListOfStringsParameter(string name, string defaultVariableName = "")
        : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => InputMode == SettingInputMode.Variable
            ? BlockSettingFactory.CreateListOfStringsSetting(Name, DefaultVariableName,
                PrettyName, Description)
            : BlockSettingFactory.CreateListOfStringsSetting(Name, DefaultValue, InputMode,
                PrettyName, Description);
}
