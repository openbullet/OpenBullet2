using FluentValidation;

namespace OpenBullet2.Web.Dtos.Shared;

/// <summary>
/// DTO that represents a shared endpoint.
/// </summary>
public class EndpointDto
{
    /// <summary>
    /// The route of this endpoint in the URI.
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// The valid API keys that can be used to access this endpoint.
    /// </summary>
    public IEnumerable<string> ApiKeys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The IDs of the configs that this endpoint should expose.
    /// </summary>
    public IEnumerable<string> ConfigIds { get; set; } = Array.Empty<string>();
}

internal class EndpointDtoValidator : AbstractValidator<EndpointDto>
{
    public EndpointDtoValidator()
    {
        RuleFor(dto => dto.Route).NotEmpty().Matches(@"^[\w-]+$");
        RuleFor(dto => dto.ApiKeys).NotNull();
        RuleFor(dto => dto.ConfigIds).NotNull();
    }
}
