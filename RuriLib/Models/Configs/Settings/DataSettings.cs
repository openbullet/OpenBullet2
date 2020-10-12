using RuriLib.Models.Data;
using System.Collections.Generic;

namespace RuriLib.Models.Configs.Settings
{
    public class DataSettings
    {
        public string[] AllowedWordlistTypes { get; set; } = new string[] { "Default" };
        public bool UrlEncodeDataAfterSlicing { get; set; } = false;
        public List<DataRule> DataRules { get; set; } = new List<DataRule>();
    }
}
