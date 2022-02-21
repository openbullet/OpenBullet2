namespace RuriLib.Models.Blocks.Settings
{
    public class StringSetting : Setting
    {
        public string Value { get; set; }
        public bool MultiLine { get; set; } = false;
    }
}
