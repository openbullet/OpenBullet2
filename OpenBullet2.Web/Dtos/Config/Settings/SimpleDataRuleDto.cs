using RuriLib.Models.Data.Rules;
using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// A data rule that checks whether a slice matches a simple condition.
/// </summary>
public class SimpleDataRuleDto
{
    /// <summary>
    /// The name of the slice that should be checked.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string SliceName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the rule should be inverted, as in the rule is valid when
    /// the slice is NOT matched by the regular expression.
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// The comparison method to use.
    /// </summary>
    [Required]
    public StringRule Comparison { get; set; } = StringRule.EqualTo;

    /// <summary>
    /// The string to compare to.
    /// </summary>
    [Required]
    public string StringToCompare { get; set; } = string.Empty;

    /// <summary>
    /// Whether the comparison should consider the letter casing.
    /// </summary>
    public bool CaseSensitive { get; set; } = true;
}
