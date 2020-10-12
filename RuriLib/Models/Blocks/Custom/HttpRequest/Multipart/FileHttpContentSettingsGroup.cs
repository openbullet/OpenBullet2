using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class FileHttpContentSettingsGroup : HttpContentSettingsGroup
    {
        public BlockSetting FileName { get; set; }

        public FileHttpContentSettingsGroup()
        {
            FileName = BlockSettingFactory.CreateStringSetting("fileName");
            ((StringSetting)ContentType.FixedSetting).Value = "application/octet-stream";
        }
    }
}
