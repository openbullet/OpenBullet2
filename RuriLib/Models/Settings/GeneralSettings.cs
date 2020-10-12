using System.Collections.Generic;

namespace RuriLib.Models.Settings
{
    public class GeneralSettings
    {
        public bool RestrictBlocksToCWD { get; set; } = true;
        public bool UseCustomUserAgentsList { get; set; } = false;
        public List<string> UserAgents { get; set; } = new List<string>
        {

        };
    }
}
