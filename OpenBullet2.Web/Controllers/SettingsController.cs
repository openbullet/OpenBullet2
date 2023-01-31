using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Settings;
using RuriLib.Models.Environment;
using RuriLib.Models.Settings;
using RuriLib.Services;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage settings.
/// </summary>
[ApiVersion("1.0")]
public class SettingsController : ApiController
{
    private readonly RuriLibSettingsService _ruriLibSettingsService;
    private readonly OpenBulletSettingsService _obSettingsService;
    private readonly IMapper _mapper;

    /// <summary></summary>
    public SettingsController(RuriLibSettingsService ruriLibSettingsService,
        OpenBulletSettingsService obSettingsService, IMapper mapper)
    {
        _ruriLibSettingsService = ruriLibSettingsService;
        _obSettingsService = obSettingsService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get the environment settings.
    /// </summary>
    [Guest]
    [HttpGet("environment")]
    [MapToApiVersion("1.0")]
    public ActionResult<EnvironmentSettingsDto> GetEnvironmentSettings()
        => _mapper.Map<EnvironmentSettingsDto>(
            _ruriLibSettingsService.Environment);

    /// <summary>
    /// Get the RuriLib settings.
    /// </summary>
    [Admin]
    [HttpGet("rurilib")]
    [MapToApiVersion("1.0")]
    public ActionResult<GlobalSettings> GetRuriLibSettings()
        => _ruriLibSettingsService.RuriLibSettings;

    /// <summary>
    /// Update the RuriLib settings.
    /// </summary>
    [Admin]
    [HttpPut("rurilib")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GlobalSettings>> UpdateRuriLibSettings(
        GlobalSettings settings)
    {
        // NOTE: We use the mapper here to apply the new settings over the
        // existing ones so we don't update the references and we can
        // edit the settings live if a component is using them.
        
        // NOTE: To check this we can just print the hashcodes before
        // and after this instruction.
        _mapper.Map(settings, _ruriLibSettingsService.RuriLibSettings);
        await _ruriLibSettingsService.Save();

        return _ruriLibSettingsService.RuriLibSettings;
    }

    /// <summary>
    /// Get the OpenBullet settings.
    /// </summary>
    /// <returns></returns>
    [Admin]
    [HttpGet]
    [MapToApiVersion("1.0")]
    public ActionResult<OpenBulletSettingsDto> GetSettings()
        => _mapper.Map<OpenBulletSettingsDto>(_obSettingsService.Settings);

    /// <summary>
    /// Update the OpenBullet settings.
    /// </summary>
    [Admin]
    [HttpPut]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<OpenBulletSettingsDto>> UpdateSettings(
        OpenBulletSettingsDto settings)
    {
        // NOTE: We use the mapper here to apply the new settings over the
        // existing ones so we don't update the references and we can
        // edit the settings live if a component is using them.

        // NOTE: To check this we can just print the hashcodes before
        // and after this instruction.
        _mapper.Map(settings, _obSettingsService.Settings);
        await _obSettingsService.Save();

        return _mapper.Map<OpenBulletSettingsDto>(_obSettingsService.Settings);
    }

    /// <summary>
    /// Update the password of the admin user. Note that this does NOT
    /// invalidate the tokens that were granted so far.
    /// </summary>
    [Admin]
    [HttpPatch("admin/password")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> UpdateAdminPassword(UpdateAdminPasswordDto dto)
    {
        _obSettingsService.Settings.SecuritySettings
            .SetupAdminPassword(dto.Password);
        await _obSettingsService.Save();

        return Ok();
    }
}
