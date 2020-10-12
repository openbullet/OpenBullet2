using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class RawHttpContentSettingsGroup : HttpContentSettingsGroup
    {
        public BlockSetting Data { get; set; }

        public RawHttpContentSettingsGroup()
        {
            Data = new BlockSetting() {
                Name = "data", 
                FixedSetting = new ByteArraySetting { Value = new byte[0] }
            };

            ((StringSetting)ContentType.FixedSetting).Value = "application/octet-stream";
        }
    }
}
