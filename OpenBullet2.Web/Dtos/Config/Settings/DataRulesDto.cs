namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains data rules for slices.
/// </summary>
public class DataRulesDto
{
    /// <summary>
    /// Simple data rules.
    /// </summary>
    public List<SimpleDataRuleDto> Simple { get; set; } = new();

    /// <summary>
    /// Regular expression-based data rules.
    /// </summary>
    public List<RegexDataRuleDto> Regex { get; set; } = new();
}
