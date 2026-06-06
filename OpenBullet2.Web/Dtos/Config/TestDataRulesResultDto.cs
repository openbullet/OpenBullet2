namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// Result of testing a data line against a wordlist type and data rules.
/// </summary>
public class TestDataRulesResultDto
{
    /// <summary>
    /// The selected wordlist type name.
    /// </summary>
    public string WordlistType { get; set; } = "Default";

    /// <summary>
    /// The regex validation result for the selected wordlist type.
    /// </summary>
    public TestDataRegexValidationDto RegexValidation { get; set; } = new();

    /// <summary>
    /// The slices produced by the selected wordlist type.
    /// </summary>
    public List<TestDataRuleSliceDto> Slices { get; set; } = [];

    /// <summary>
    /// The evaluation results for each data rule.
    /// </summary>
    public List<TestDataRuleResultDto> Results { get; set; } = [];
}

/// <summary>
/// Result of validating the input data against a wordlist regex.
/// </summary>
public class TestDataRegexValidationDto
{
    /// <summary>
    /// Whether the input passed regex validation.
    /// </summary>
    public bool Passed { get; set; }
}

/// <summary>
/// One slice produced by splitting the input data.
/// </summary>
public class TestDataRuleSliceDto
{
    /// <summary>
    /// The slice name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The slice value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Result of evaluating a single data rule.
/// </summary>
public class TestDataRuleResultDto
{
    /// <summary>
    /// Whether the rule passed.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Human-readable description of the rule evaluation.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
