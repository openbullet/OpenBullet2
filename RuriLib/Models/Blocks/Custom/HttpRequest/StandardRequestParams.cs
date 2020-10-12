using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest
{
    public class StandardRequestParams : RequestParams
    {
        public BlockSetting Content { get; set; } = BlockSettingFactory.CreateStringSetting("content", string.Empty, SettingInputMode.Interpolated);
        public BlockSetting ContentType { get; set; } = BlockSettingFactory.CreateStringSetting("contentType", "application/x-www-form-urlencoded");
    }
}
