using System.Collections.Generic;
using RuriLib.Helpers.CSharp;

namespace RuriLib.Models.Configs.Settings
{
    public class ScriptSettings
    {
        /// <summary>
        /// Defines the additional using statements that the <see cref="ScriptBuilder"/> will use
        /// when building the final script for execution.
        /// </summary>
        public List<string> CustomUsings { get; set; } = new List<string>();
    }
}
