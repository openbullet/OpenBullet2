using System;

namespace RuriLib.Models.Environment;

/// <summary>
/// Describes how a wordlist line should be validated and sliced.
/// </summary>
public class WordlistType
{
    /// <summary>The name of the Wordlist Type.</summary>
    public string Name { get; set; } = "Default";

    /// <summary>The regular expression that validates the input data.</summary>
    public string Regex { get; set; } = ".*";

    /// <summary>Whether to check if the regex successfully matches the input data.</summary>
    public bool Verify { get; set; }

    /// <summary>The separator used for slicing the input data into a list of strings.</summary>
    public string Separator { get; set; } = ":";

    /// <summary>The list of names of the variable that will be created by slicing the input data.</summary>
    public string[] Slices { get; set; } = ["DATA"];

    /// <summary>Alias for the list of names of the variable that will be created by slicing the input data.</summary>
    public string[] SlicesAlias { get; set; } = Array.Empty<string>();
}
