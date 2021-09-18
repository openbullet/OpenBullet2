using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Custom
{
    public class ParseBlockDescriptor : BlockDescriptor
    {
        public ParseBlockDescriptor()
        {
            Id = "Parse";
            Name = Id;
            Description = "Parses text from a string";
            Category = new BlockCategory
            {
                Name = "Parsing",
                BackgroundColor = "#ffd700",
                ForegroundColor = "#000",
                Path = "RuriLib.Blocks.Parsing",
                Namespace = "RuriLib.Blocks.Parsing.Methods",
                Description = "Blocks for extracting data from strings"
            };

            Parameters = new Dictionary<string, BlockParameter>
            {
                { "input", new StringParameter("input", "data.SOURCE", SettingInputMode.Variable) },
                { "prefix", new StringParameter("prefix") },
                { "suffix", new StringParameter("suffix") },
                { "urlEncodeOutput", new BoolParameter("urlEncodeOutput", false) },

                // LR
                { "leftDelim", new StringParameter("leftDelim") },
                { "rightDelim", new StringParameter("rightDelim") },
                { "caseSensitive", new BoolParameter("caseSensitive", true) },
                
                // CSS
                { "cssSelector", new StringParameter("cssSelector") },
                { "attributeName", new StringParameter("attributeName", "innerText") },

                // XPATH
                { "xPath", new StringParameter("xPath") },

                // JSON
                { "jToken", new StringParameter("jToken") },

                // REGEX
                { "pattern", new StringParameter("pattern") },
                { "outputFormat", new StringParameter("outputFormat") },
                { "multiLine", new BoolParameter("multiLine", false) }
            };
        }
    }
}
