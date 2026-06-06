using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RuriLib.Models.Configs.Settings;

/// <summary>
/// Configures data slicing, validation, and resources.
/// </summary>
public class DataSettings
{
    /// <summary>
    /// The allowed wordlist types for the config.
    /// </summary>
    public string[] AllowedWordlistTypes { get; set; } = ["Default"];

    /// <summary>
    /// Whether sliced data should be URL-encoded.
    /// </summary>
    public bool UrlEncodeDataAfterSlicing { get; set; }

    /// <summary>
    /// Rules that must be respected by input data.
    /// </summary>
    public List<DataRule> DataRules { get; set; } = [];

    /// <summary>
    /// Additional named resources available during execution.
    /// </summary>
    public List<ConfigResourceOptions> Resources { get; set; } = [];

    /// <summary>
    /// A display-friendly concatenation of <see cref="AllowedWordlistTypes"/>.
    /// </summary>
    [JsonIgnore]
    public string AllowedWordlistTypesString => string.Join(", ", AllowedWordlistTypes);
}
