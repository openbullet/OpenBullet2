using FluentValidation;

namespace OpenBullet2.Web.Dtos.ProxyGroup;

/// <summary>
/// DTO to update a proxy group.
/// </summary>
public class UpdateProxyGroupDto
{
    /// <summary>
    /// The id of the proxy group.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// The name of the proxy group.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

internal class UpdateProxyGroupDtoValidator : AbstractValidator<UpdateProxyGroupDto>
{
    public UpdateProxyGroupDtoValidator()
    {
        RuleFor(dto => dto.Id).GreaterThan(0);
        RuleFor(dto => dto.Name).NotEmpty().Length(3, 32);
    }
}
