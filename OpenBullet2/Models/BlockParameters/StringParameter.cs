using OpenBullet2.Models.Settings;

namespace OpenBullet2.Models.BlockParameters
{
    public class StringParameter : BlockParameter
    {
        public string DefaultValue { get; set; }

        public override Setting ToSetting() => new StringSetting { Name = Name, Value = DefaultValue };
    }
}
