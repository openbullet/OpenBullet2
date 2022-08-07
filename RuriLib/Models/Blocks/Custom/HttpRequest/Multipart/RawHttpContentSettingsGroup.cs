using RuriLib.Models.Blocks.Settings;
using System;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class RawHttpContentSettingsGroup : HttpContentSettingsGroup
    {
        public BlockSetting Data { get; set; }

        public RawHttpContentSettingsGroup()
        {
            Data = new BlockSetting() {
                Name = "data", 
                FixedSetting = new ByteArraySetting { Value = Array.Empty<byte>() }
            };

            ((StringSetting)ContentType.FixedSetting).Value = "application/octet-stream";
        }
    }
}
