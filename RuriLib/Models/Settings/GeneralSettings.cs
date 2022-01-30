using RuriLib.Parallelization;
using System.Collections.Generic;

namespace RuriLib.Models.Settings
{
    public class GeneralSettings
    {
        public ParallelizerType ParallelizerType { get; set; } = ParallelizerType.TaskBased;
        public bool LogJobActivityToFile { get; set; } = false;
        public bool RestrictBlocksToCWD { get; set; } = true;
        public bool UseCustomUserAgentsList { get; set; } = false;
        public bool EnableBotLogging { get; set; } = false;
        public bool VerboseMode { get; set; } = false;
        public bool LogAllResults { get; set; } = false;
        public List<string> UserAgents { get; set; } = new List<string>
        {

        };
    }
}
