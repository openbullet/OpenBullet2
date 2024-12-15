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
    public int DefaultValue { get; set; }

    /// <summary></summary>
    public IntParameter(string name) : base(name)
    {

    }

    /// <summary></summary>
    public IntParameter(string name, int defaultValue = 0) : base(name)
    {
        DefaultValue = defaultValue;
    }

    /// <summary></summary>
    public IntParameter(string name, string defaultVariableName = "") : base(name)
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
            FixedSetting = new IntSetting { Value = DefaultValue },
            InputMode = InputMode
        };
}
