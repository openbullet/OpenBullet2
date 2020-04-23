using OpenBullet2.Models.Settings;

namespace OpenBullet2.Models.BlockParameters
{
    public class BoolParameter : BlockParameter
    {
        public bool DefaultValue { get; set; }

        public override Setting ToSetting() => new BoolSetting { Name = Name, Value = DefaultValue };
    }
}
