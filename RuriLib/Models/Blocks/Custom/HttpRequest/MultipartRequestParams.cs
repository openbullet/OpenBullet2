using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Custom.HttpRequest
{
    public class MultipartRequestParams : RequestParams
    {
        public List<HttpContentSettingsGroup> Contents = new List<HttpContentSettingsGroup>();
        public BlockSetting Boundary { get; set; } = BlockSettingFactory.CreateStringSetting("boundary");
    }
}
