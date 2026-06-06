using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of type int.
/// </summary>
public class IntParameter : BlockParameter
{
    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public long DefaultValue { get; set; }

    /// <summary>
    /// Whether generated code should pass this parameter as a <see cref="long"/> instead of an <see cref="int"/>.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public bool UseLong { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    public IntParameter(string name) : base(name)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    public IntParameter(string name, int defaultValue = 0) : base(name)
    {
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    public IntParameter(string name, long defaultValue) : base(name)
    {
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    public IntParameter(string name, string defaultVariableName = "") : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => BlockSettingFactory.CreateIntSetting(Name, DefaultValue, InputMode,
            DefaultVariableName, PrettyName, Description, UseLong);
}
