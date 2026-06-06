using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of type string.
/// </summary>
public class StringParameter : BlockParameter
{
    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Whether the string should be displayed as a multiline textbox.
    /// </summary>
    public bool MultiLine { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    public StringParameter(string name) : base(name)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="inputMode">The supported input mode.</param>
    public StringParameter(string name,
        string? defaultValue = "", SettingInputMode inputMode = SettingInputMode.Fixed)
        : base(name)
    {
        DefaultValue = defaultValue ?? string.Empty;
        InputMode = inputMode;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => BlockSettingFactory.CreateStringSetting(Name, DefaultValue, InputMode, MultiLine, PrettyName, Description);
}
