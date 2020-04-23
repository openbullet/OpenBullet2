using OpenBullet2.Models.Settings;

namespace OpenBullet2.Models.BlockParameters
{
    public class IntParameter : BlockParameter
    {
        public int DefaultValue { get; set; }

        public override Setting ToSetting() => new IntSetting { Name = Name, Value = DefaultValue };
    }
}
