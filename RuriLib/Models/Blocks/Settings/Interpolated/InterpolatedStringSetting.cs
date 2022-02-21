namespace RuriLib.Models.Blocks.Settings.Interpolated
{
    public class InterpolatedStringSetting : InterpolatedSetting
    {
        public string Value { get; set; } = string.Empty;
        public bool MultiLine { get; set; } = false;
    }
}
