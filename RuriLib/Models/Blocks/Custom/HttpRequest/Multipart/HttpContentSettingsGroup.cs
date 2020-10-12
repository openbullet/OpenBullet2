using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class HttpContentSettingsGroup
    {
        public BlockSetting Name { get; set; } = BlockSettingFactory.CreateStringSetting("name");
        public BlockSetting ContentType { get; set; } = BlockSettingFactory.CreateStringSetting("contentType");
    }
}
