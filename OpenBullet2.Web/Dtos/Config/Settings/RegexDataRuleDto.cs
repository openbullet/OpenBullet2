using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// A data rule that checks whether a slice matches a regular expression.
/// </summary>
public class RegexDataRuleDto
{
    /// <summary>
    /// The name of the slice that should be checked.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string SliceName { get; set; } = string.Empty;

    /// <summary>
    /// The regular expression that should be matched by the slice.
    /// </summary>
    [Required]
    public string RegexToMatch { get; set; } = string.Empty;

    /// <summary>
    /// Whether the rule should be inverted, as in the rule is valid when
    /// the slice is NOT matched by the regular expression.
    /// </summary>
    public bool Invert { get; set; } = false;
}
