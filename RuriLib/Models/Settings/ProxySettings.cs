using System.Collections.Generic;

namespace RuriLib.Models.Settings
{
    public class ProxySettings
    {
        public List<string> GlobalBanKeys { get; set; } = new List<string>();
        public List<string> GlobalRetryKeys { get; set; } = new List<string>();
    }
}
