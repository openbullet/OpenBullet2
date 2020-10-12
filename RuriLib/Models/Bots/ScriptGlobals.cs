using RuriLib.Models.Variables;
using System.Collections.Generic;
using System.Dynamic;

namespace RuriLib.Models.Bots
{
    /// <summary>
    /// Global variables accessible by the Roslyn script.
    /// </summary>
    public class ScriptGlobals
    {
        /// <summary>
        /// The data of the bot, such as the current DataLine or Proxy being used.
        /// </summary>
        public BotData data;

        /// <summary>
        /// The expando object where each field is a slice of the original data line.
        /// </summary>
        public dynamic input;

        public ScriptGlobals(BotData data)
        {
            this.data = data;

            input = new ExpandoObject();
            foreach (var variable in data.Line.GetVariables())
                ((IDictionary<string, object>)input).Add(variable.Name, variable.AsString());
        }
    }
}
