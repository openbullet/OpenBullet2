using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest
{
    public class BasicAuthRequestParams : RequestParams
    {
        public BlockSetting Username { get; set; } = BlockSettingFactory.CreateStringSetting("username");
        public BlockSetting Password { get; set; } = BlockSettingFactory.CreateStringSetting("password");
    }
}
