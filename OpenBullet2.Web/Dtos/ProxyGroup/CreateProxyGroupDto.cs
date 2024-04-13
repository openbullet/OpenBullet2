using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace OpenBullet2.Web.Dtos.ProxyGroup;

/// <summary>
/// DTO to create a new proxy group.
/// </summary>
public class CreateProxyGroupDto
{
    /// <summary>
    /// The name of the proxy group.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

internal class CreateProxyGroupDtoValidator : AbstractValidator<CreateProxyGroupDto>
{
    public CreateProxyGroupDtoValidator()
    {
        RuleFor(dto => dto.Name).NotEmpty().Length(3, 32);
    }
}
