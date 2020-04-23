using OpenBullet2.Models.Settings;

namespace OpenBullet2.Models.BlockParameters
{
    public class FloatParameter : BlockParameter
    {
        public float DefaultValue { get; set; }

        public override Setting ToSetting() => new FloatSetting { Name = Name, Value = DefaultValue };
    }
}
