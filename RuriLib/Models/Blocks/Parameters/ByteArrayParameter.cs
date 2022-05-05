using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using System;

namespace RuriLib.Models.Blocks.Parameters
{
    public class ByteArrayParameter : BlockParameter
    {
        public byte[] DefaultValue { get; set; }

        public ByteArrayParameter()
        {

        }

        public ByteArrayParameter(string name, byte[] defaultValue = null, SettingInputMode inputMode = SettingInputMode.Fixed)
        {
            Name = name;
            InputMode = inputMode;
            DefaultValue = defaultValue ?? Array.Empty<byte>();
        }

        public ByteArrayParameter(string name, string defaultVariableName = "")
        {
            Name = name;
            DefaultVariableName = defaultVariableName;
            DefaultValue = Array.Empty<byte>();
            InputMode = SettingInputMode.Variable;
        }

        public override BlockSetting ToBlockSetting()
            => new()
            {
                Name = Name,
                Description = Description,
                ReadableName = PrettyName ?? Name.ToReadableName(),
                FixedSetting = new ByteArraySetting { Value = DefaultValue },
                InputMode = InputMode
            };
    }
}
