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
    public float DefaultValue { get; set; }

    /// <summary></summary>
    public FloatParameter(string name) : base(name)
    {

    }

    /// <summary></summary>
    public FloatParameter(string name, float defaultValue = 0) : base(name)
    {
        DefaultValue = defaultValue;
    }

    /// <summary></summary>
    public FloatParameter(string name, string defaultVariableName = "") : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => new()
        {
            Name = Name,
            Description = Description,
            ReadableName = PrettyName,
            FixedSetting = new FloatSetting { Value = DefaultValue },
            InputMode = InputMode
        };
}
