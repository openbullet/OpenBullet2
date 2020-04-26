using System;
using System.Collections.Generic;

namespace OpenBullet2.Models.Configs
{
    public class Config
    {
        public string Id { get; set; }
        public ConfigMetadata Metadata { get; set; } = new ConfigMetadata();
        public ConfigSettings Settings { get; set; } = new ConfigSettings();
        public string Readme { get; set; } = "Type some **markdown** here";
        public string CSharpScript { get; set; } = ""; // Put a default here
        public List<BlockInstance> Blocks { get; set; } = new List<BlockInstance>();

        public Config()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
