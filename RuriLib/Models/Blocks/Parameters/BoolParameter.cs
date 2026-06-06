using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of type bool.
/// </summary>
public class BoolParameter : BlockParameter
{
    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public bool DefaultValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoolParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    public BoolParameter(string name) : base(name)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoolParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    public BoolParameter(string name, bool defaultValue = false) : base(name)
    {
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoolParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    public BoolParameter(string name, string defaultVariableName = "") : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => BlockSettingFactory.CreateBoolSetting(Name, DefaultValue, InputMode,
            DefaultVariableName, PrettyName, Description);
}
