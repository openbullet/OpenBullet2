using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using System;

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

    /// <summary></summary>
    public ByteArrayParameter(string name) : base(name)
    {

    }

    /// <summary></summary>
    public ByteArrayParameter(string name, byte[]? defaultValue = null,
        SettingInputMode inputMode = SettingInputMode.Fixed) : base(name)
    {
        InputMode = inputMode;
        DefaultValue = defaultValue ?? [];
    }

    /// <summary></summary>
    public ByteArrayParameter(string name, string defaultVariableName = "")
        : base(name)
    {
        Name = name;
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
            FixedSetting = new ByteArraySetting { Value = DefaultValue },
            InputMode = InputMode
        };
}
