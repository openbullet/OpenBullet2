using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of type float.
/// </summary>
public class FloatParameter : BlockParameter
{
    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public double DefaultValue { get; set; }

    /// <summary>
    /// Whether generated code should pass this parameter as a <see cref="double"/> instead of a <see cref="float"/>.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public bool UseDouble { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    public FloatParameter(string name) : base(name)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    public FloatParameter(string name, float defaultValue = 0) : base(name)
    {
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    public FloatParameter(string name, double defaultValue) : base(name)
    {
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    public FloatParameter(string name, string defaultVariableName = "") : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => BlockSettingFactory.CreateFloatSetting(Name, DefaultValue, InputMode,
            DefaultVariableName, PrettyName, Description, UseDouble);
}
