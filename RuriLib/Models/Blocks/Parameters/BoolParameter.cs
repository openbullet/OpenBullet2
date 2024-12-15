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

    /// <summary></summary>
    public BoolParameter(string name) : base(name)
    {

    }

    /// <summary></summary>
    public BoolParameter(string name, bool defaultValue = false) : base(name)
    {
        Name = name;
        DefaultValue = defaultValue;
    }

    /// <summary></summary>
    public BoolParameter(string name, string defaultVariableName = "") : base(name)
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
            FixedSetting = new BoolSetting { Value = DefaultValue },
            InputMode = InputMode
        };
}
