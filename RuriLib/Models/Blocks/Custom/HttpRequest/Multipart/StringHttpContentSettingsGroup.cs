using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class StringHttpContentSettingsGroup : HttpContentSettingsGroup
    {
        public BlockSetting Data { get; set; }

        public StringHttpContentSettingsGroup()
        {
            Data = BlockSettingFactory.CreateStringSetting("data");
            ((StringSetting)ContentType.FixedSetting).Value = "text/plain";
        }
    }
}
