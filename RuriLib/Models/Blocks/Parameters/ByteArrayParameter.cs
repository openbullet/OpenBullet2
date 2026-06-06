using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of type byte array.
/// </summary>
public class ByteArrayParameter : BlockParameter
{
    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public byte[] DefaultValue { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteArrayParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    public ByteArrayParameter(string name) : base(name)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteArrayParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultValue">The default fixed value.</param>
    /// <param name="inputMode">The supported input mode.</param>
    public ByteArrayParameter(string name, byte[]? defaultValue = null,
        SettingInputMode inputMode = SettingInputMode.Fixed) : base(name)
    {
        InputMode = inputMode;
        DefaultValue = defaultValue ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteArrayParameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="defaultVariableName">The default variable name when used in variable mode.</param>
    public ByteArrayParameter(string name, string defaultVariableName = "")
        : base(name)
    {
        DefaultVariableName = defaultVariableName;
        InputMode = SettingInputMode.Variable;
    }

    /// <inheritdoc />
    public override BlockSetting ToBlockSetting()
        => BlockSettingFactory.CreateByteArraySetting(Name, DefaultValue, InputMode,
            DefaultVariableName, PrettyName, Description);
}
