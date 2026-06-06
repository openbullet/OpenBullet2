using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom;

/// <summary>
/// Descriptor for the parse block.
/// </summary>
public class ParseBlockDescriptor : BlockDescriptor
{
    /// <summary>
    /// Initializes a new <see cref="ParseBlockDescriptor"/>.
    /// </summary>
    public ParseBlockDescriptor()
    {
        Id = "Parse";
        Name = Id;
        Description = "Parses text from a string";
        Category = new()
        {
            Name = "Parsing",
            BackgroundColor = "#ffd700",
            ForegroundColor = "#000",
            Path = "RuriLib.Blocks.Parsing",
            Namespace = "RuriLib.Blocks.Parsing.Methods",
            Description = "Blocks for extracting data from strings"
        };

        Parameters = new()
        {
            ["input"] = new StringParameter("input", "data.SOURCE", SettingInputMode.Variable),
            ["prefix"] = new StringParameter("prefix")
            {
                Description = "Prepended to each parsed result before the final optional URL encoding."
            },
            ["suffix"] = new StringParameter("suffix")
            {
                Description = "Appended to each parsed result before the final optional URL encoding."
            },
            ["urlEncodeOutput"] = new BoolParameter("urlEncodeOutput", false)
            {
                Description = "If true, URL-encodes each final parsed result after prefix and suffix are applied."
            },
            ["leftDelim"] = new StringParameter("leftDelim")
            {
                Description = "Left delimiter used only in LR mode. If empty, it uses the start of the string."
            },
            ["rightDelim"] = new StringParameter("rightDelim")
            {
                Description = "Right delimiter used only in LR mode. If empty, it uses the end of the string."
            },
            ["caseSensitive"] = new BoolParameter("caseSensitive", true)
            {
                Description = "Whether LR mode should match the left and right delimiters case-sensitively."
            },
            ["cssSelector"] = new StringParameter("cssSelector")
            {
                Description = "CSS selector used only in CSS mode."
            },
            ["attributeName"] = new StringParameter("attributeName", "innerText")
            {
                Description = "Attribute or pseudo-field to extract in CSS/XPath mode. Common values are innerText, innerHtml, outerHtml, href, src, value, or any actual attribute name."
            },
            ["xPath"] = new StringParameter("xPath")
            {
                Description = "XPath expression used only in XPath mode."
            },
            ["jToken"] = new StringParameter("jToken")
            {
                Description = "JSON token path used only in JSON mode."
            },
            ["pattern"] = new StringParameter("pattern")
            {
                Description = "Regex pattern used only in Regex mode."
            },
            ["outputFormat"] = new StringParameter("outputFormat")
            {
                Description = "Regex output template used only in Regex mode. [0] is the full match, [1], [2], ... are capture groups."
            },
            ["multiLine"] = new BoolParameter("multiLine", false)
            {
                Description = "If true, enables RegexOptions.Multiline in Regex mode."
            }
        };
    }
}
