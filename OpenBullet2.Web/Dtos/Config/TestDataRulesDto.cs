using OpenBullet2.Web.Dtos.Config.Settings;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// Request payload for testing a data line against a wordlist type and data rules.
/// </summary>
public class TestDataRulesDto
{
    /// <summary>
    /// The raw input data to test.
    /// </summary>
    public string TestData { get; set; } = string.Empty;

    /// <summary>
    /// The selected wordlist type name.
    /// </summary>
    public string WordlistType { get; set; } = "Default";

    /// <summary>
    /// The data rules to evaluate.
    /// </summary>
    public DataRulesDto DataRules { get; set; } = new();
}
