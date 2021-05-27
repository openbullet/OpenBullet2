using RuriLib.Models.Data.Rules;
using RuriLib.Models.Data.Resources.Options;
using System.Collections.Generic;

namespace RuriLib.Models.Configs.Settings
{
    public class DataSettings
    {
        public string[] AllowedWordlistTypes { get; set; } = new string[] { "Default" };
        public bool UrlEncodeDataAfterSlicing { get; set; } = false;
        public List<DataRule> DataRules { get; set; } = new List<DataRule>();
        public List<ConfigResourceOptions> Resources { get; set; } = new List<ConfigResourceOptions>();

        public string AllowedWordlistTypesString => string.Join(", ", AllowedWordlistTypes);
    }
}
