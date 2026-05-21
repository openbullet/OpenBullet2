using FluentValidation;
using OpenBullet2.Web.Dtos.Config.Settings;
using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains a config's settings.
/// </summary>
public class ConfigSettingsDto
{
    /// <summary>
    /// The general settings.
    /// </summary>
    [Required]
    public ConfigGeneralSettingsDto GeneralSettings { get; set; } = new();

    /// <summary>
    /// The proxy-related settings.
    /// </summary>
    [Required]
    public ConfigProxySettingsDto ProxySettings { get; set; } = new();

    /// <summary>
    /// The input settings.
    /// </summary>
    [Required]
    public ConfigInputSettingsDto InputSettings { get; set; } = new();

    /// <summary>
    /// The data-related settings.
    /// </summary>
    [Required]
    public ConfigDataSettingsDto DataSettings { get; set; } = new();

    /// <summary>
    /// The browser-related settings.
    /// </summary>
    [Required]
    public ConfigBrowserSettingsDto BrowserSettings { get; set; } = new();

    /// <summary>
    /// The script-related settings.
    /// </summary>
    [Required]
    public ConfigScriptSettingsDto ScriptSettings { get; set; } = new();
}

internal class ConfigSettingsDtoValidator : AbstractValidator<ConfigSettingsDto>
{
    public ConfigSettingsDtoValidator()
    {
        RuleFor(x => x.GeneralSettings)
            .NotNull()
            .WithMessage("General settings are required.")
            .SetValidator(new ConfigGeneralSettingsDtoValidator()!);

        RuleFor(x => x.ProxySettings)
            .NotNull()
            .WithMessage("Proxy settings are required.")
            .SetValidator(new ConfigProxySettingsDtoValidator()!);

        RuleFor(x => x.InputSettings)
            .NotNull()
            .WithMessage("Input settings are required.")
            .SetValidator(new ConfigInputSettingsDtoValidator()!);

        RuleFor(x => x.DataSettings)
            .NotNull()
            .WithMessage("Data settings are required.")
            .SetValidator(new ConfigDataSettingsDtoValidator()!);

        RuleFor(x => x.BrowserSettings)
            .NotNull()
            .WithMessage("Browser settings are required.")
            .SetValidator(new ConfigBrowserSettingsDtoValidator()!);

        RuleFor(x => x.ScriptSettings)
            .NotNull()
            .WithMessage("Script settings are required.")
            .SetValidator(new ConfigScriptSettingsDtoValidator()!);
    }
}

internal class ConfigGeneralSettingsDtoValidator : AbstractValidator<ConfigGeneralSettingsDto>
{
    public ConfigGeneralSettingsDtoValidator()
    {
        RuleFor(x => x.SuggestedBots)
            .GreaterThanOrEqualTo(0)
            .WithMessage("SuggestedBots must be greater than or equal to 0.");

        RuleFor(x => x.MaximumCPM)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaximumCPM must be greater than or equal to 0.");

        RuleFor(x => x.ContinueStatuses)
            .NotNull()
            .WithMessage("ContinueStatuses cannot be null.");

        RuleForEach(x => x.ContinueStatuses)
            .NotEmpty()
            .WithMessage("ContinueStatuses cannot contain empty values.");
    }
}

internal class ConfigProxySettingsDtoValidator : AbstractValidator<ConfigProxySettingsDto>
{
    public ConfigProxySettingsDtoValidator()
    {
        RuleFor(x => x.MaxUsesPerProxy)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxUsesPerProxy must be greater than or equal to 0.");

        RuleFor(x => x.BanLoopEvasion)
            .GreaterThanOrEqualTo(0)
            .WithMessage("BanLoopEvasion must be greater than or equal to 0.");

        RuleFor(x => x.BanProxyStatuses)
            .NotNull()
            .WithMessage("BanProxyStatuses cannot be null.");

        RuleForEach(x => x.BanProxyStatuses)
            .NotEmpty()
            .WithMessage("BanProxyStatuses cannot contain empty values.");

        RuleFor(x => x.AllowedProxyTypes)
            .NotNull()
            .WithMessage("AllowedProxyTypes cannot be null.");
    }
}

internal class ConfigInputSettingsDtoValidator : AbstractValidator<ConfigInputSettingsDto>
{
    public ConfigInputSettingsDtoValidator()
    {
        RuleFor(x => x.CustomInputs)
            .NotNull()
            .WithMessage("CustomInputs cannot be null.");

        RuleForEach(x => x.CustomInputs)
            .SetValidator(new CustomInputDtoValidator());
    }
}

internal class CustomInputDtoValidator : AbstractValidator<CustomInputDto>
{
    public CustomInputDtoValidator()
    {
        RuleFor(x => x.VariableName)
            .NotEmpty()
            .WithMessage("Custom input VariableName is required.");
    }
}

internal class ConfigDataSettingsDtoValidator : AbstractValidator<ConfigDataSettingsDto>
{
    public ConfigDataSettingsDtoValidator()
    {
        RuleFor(x => x.AllowedWordlistTypes)
            .NotNull()
            .WithMessage("AllowedWordlistTypes cannot be null.")
            .Must(types => types.Length > 0)
            .WithMessage("AllowedWordlistTypes must contain at least one value.");

        RuleForEach(x => x.AllowedWordlistTypes)
            .NotEmpty()
            .WithMessage("AllowedWordlistTypes cannot contain empty values.");

        RuleFor(x => x.DataRules)
            .NotNull()
            .WithMessage("DataRules cannot be null.");

        RuleForEach(x => x.DataRules.Simple)
            .SetValidator(new SimpleDataRuleDtoValidator());

        RuleForEach(x => x.DataRules.Regex)
            .SetValidator(new RegexDataRuleDtoValidator());

        RuleFor(x => x.Resources)
            .NotNull()
            .WithMessage("Resources cannot be null.");
    }
}

internal class SimpleDataRuleDtoValidator : AbstractValidator<SimpleDataRuleDto>
{
    public SimpleDataRuleDtoValidator()
    {
        RuleFor(x => x.SliceName)
            .NotEmpty()
            .WithMessage("Simple data rule SliceName is required.");
    }
}

internal class RegexDataRuleDtoValidator : AbstractValidator<RegexDataRuleDto>
{
    public RegexDataRuleDtoValidator()
    {
        RuleFor(x => x.SliceName)
            .NotEmpty()
            .WithMessage("Regex data rule SliceName is required.");

        RuleFor(x => x.RegexToMatch)
            .NotEmpty()
            .WithMessage("Regex data rule RegexToMatch is required.");
    }
}

internal class ConfigBrowserSettingsDtoValidator : AbstractValidator<ConfigBrowserSettingsDto>
{
    public ConfigBrowserSettingsDtoValidator()
    {
        RuleFor(x => x.Engine)
            .IsInEnum()
            .WithMessage("Engine is invalid.");

        RuleFor(x => x.QuitBrowserStatuses)
            .NotNull()
            .WithMessage("QuitBrowserStatuses cannot be null.");

        RuleForEach(x => x.QuitBrowserStatuses)
            .NotEmpty()
            .WithMessage("QuitBrowserStatuses cannot contain empty values.");

        RuleFor(x => x.BlockedUrls)
            .NotNull()
            .WithMessage("BlockedUrls cannot be null.");

        RuleForEach(x => x.BlockedUrls)
            .NotEmpty()
            .WithMessage("BlockedUrls cannot contain empty values.");
    }
}

internal class ConfigScriptSettingsDtoValidator : AbstractValidator<ConfigScriptSettingsDto>
{
    public ConfigScriptSettingsDtoValidator()
    {
        RuleFor(x => x.CustomUsings)
            .NotNull()
            .WithMessage("CustomUsings cannot be null.");

        RuleForEach(x => x.CustomUsings)
            .NotEmpty()
            .WithMessage("CustomUsings cannot contain empty values.");
    }
}
