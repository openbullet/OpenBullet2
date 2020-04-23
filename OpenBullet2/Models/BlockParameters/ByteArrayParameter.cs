using OpenBullet2.Models.Settings;

namespace OpenBullet2.Models.BlockParameters
{
    public class ByteArrayParameter : BlockParameter
    {
        public byte[] DefaultValue { get; set; }

        public override Setting ToSetting() => new ByteArraySetting { Name = Name, Value = DefaultValue };
    }
}
