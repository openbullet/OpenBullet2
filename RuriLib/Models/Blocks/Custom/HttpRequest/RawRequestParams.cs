using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest
{
    public class RawRequestParams : RequestParams
    {
        public BlockSetting Content { get; set; } = new BlockSetting
        {
            Name = "content",
            FixedSetting = new ByteArraySetting()
        };

        public BlockSetting ContentType { get; set; } = BlockSettingFactory.CreateStringSetting("contentType", "application/octet-stream");
    }
}
