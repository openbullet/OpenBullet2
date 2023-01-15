using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Settings;
using RuriLib.Services;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage settings.
/// </summary>
[ApiVersion("1.0")]
public class SettingsController : ApiController
{
    private readonly RuriLibSettingsService _ruriLibSettingsService;
    private readonly IMapper _mapper;

    /// <summary></summary>
    public SettingsController(RuriLibSettingsService ruriLibSettingsService,
        IMapper mapper)
    {
        _ruriLibSettingsService = ruriLibSettingsService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get the environment settings.
    /// </summary>
    [Guest]
    [HttpGet("environment")]
    [MapToApiVersion("1.0")]
    public ActionResult<EnvironmentSettingsDto> GetEnvironmentSettings()
        => _mapper.Map<EnvironmentSettingsDto>(_ruriLibSettingsService.Environment);
}
