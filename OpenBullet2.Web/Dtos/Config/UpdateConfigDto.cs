using RuriLib.Models.Configs;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains a config's data.
/// </summary>
public class UpdateConfigDto
{
    /// <summary>
    /// The unique id of the config.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The current config mode.
    /// </summary>
    public ConfigMode Mode { get; set; } = ConfigMode.LoliCode;

    /// <summary>
    /// The metadata of the config.
    /// </summary>
    public UpdateConfigMetadataDto Metadata { get; set; } = default!;

    /// <summary>
    /// The config's settings.
    /// </summary>
    public ConfigSettingsDto Settings { get; set; } = default!;

    /// <summary>
    /// The markdown body of the readme.
    /// </summary>
    public string Readme { get; set; } = string.Empty;

    /// <summary>
    /// The LoliCode script.
    /// </summary>
    public string LoliCodeScript { get; set; } = string.Empty;

    /// <summary>
    /// The LoliCode script that gets executed once, before anything else.
    /// </summary>
    public string StartupLoliCodeScript { get; set; } = string.Empty;

    /// <summary>
    /// The LoliScript code of legacy configs.
    /// </summary>
    public string LoliScript { get; set; } = string.Empty;

    /// <summary>
    /// The C# script that gets executed once, before anything else.
    /// </summary>
    public string StartupCSharpScript { get; set; } = string.Empty;

    /// <summary>
    /// The C# script for configs that were converted to C#.
    /// </summary>
    public string CSharpScript { get; set; } = string.Empty;

    /// <summary>
    /// Whether to persistently save the config to the repository.
    /// Set to <see langword="false" /> if the config is only being saved to
    /// memory, e.g. for debugging needs.
    /// </summary>
    public bool Persistent { get; set; } = true;
}

internal class UpdateConfigDtoValidator : AbstractValidator<UpdateConfigDto>
{
    public UpdateConfigDtoValidator()
    {
        RuleFor(dto => dto.Id).NotEmpty();
    }
}
