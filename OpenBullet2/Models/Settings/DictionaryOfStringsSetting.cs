using System.Collections.Generic;

namespace OpenBullet2.Models.Settings
{
    public class DictionaryOfStringsSetting : Setting
    {
        public Dictionary<string, string> Value { get; set; }
    }
}
