using System;

namespace OpenBullet2.Models.Configs
{
    public class ConfigMetadata
    {
        public string Name { get; set; } = "";
        public string Author { get; set; } = "";
        public string Base64Image { get; set; } = "";
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }

        public ConfigMetadata()
        {
            CreationDate = DateTime.Now;
            LastModified = CreationDate;
        }
    }
}
