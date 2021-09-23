using RuriLib.LS;
using RuriLib.Models.Bots;
using System.Collections.Generic;

namespace RuriLib.Legacy.Models
{
    public class LSGlobals
    {
        public BotData BotData { get; set; }
        public VariablesList Inputs { get; set; }
        public VariablesList Globals { get; set; }
        public Dictionary<string, string> GlobalCookies { get; set; }
    }
}
