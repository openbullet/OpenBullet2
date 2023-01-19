using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Exceptions;
using RuriLib.Models.Configs;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage configs.
/// </summary>
[ApiVersion("1.0")]
public class ConfigController : ApiController
{
    private readonly IConfigRepository _configRepo;
    private readonly ConfigService _configService;
    private readonly IMapper _mapper;
    private readonly ILogger<ConfigController> _logger;

    /// <summary></summary>
    public ConfigController(IConfigRepository configRepo,
        ConfigService configService, IMapper mapper,
        ILogger<ConfigController> logger)
    {
        _configRepo = configRepo;
        _configService = configService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// List all of the available configs.
    /// </summary>
    [Guest]
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<ConfigInfoDto>> GetAll()
    {
        var configs = _configService.Configs
            .OrderByDescending(c => c.Metadata.LastModified);

        return Ok(_mapper.Map<IEnumerable<ConfigInfoDto>>(configs));
    }

    /// <summary>
    /// Get the readme of a config.
    /// </summary>
    [Guest]
    [HttpGet("readme")]
    [MapToApiVersion("1.0")]
    public ActionResult<ConfigReadmeDto> GetReadme(string id)
    {
        var config = GetConfigFromService(id);

        return new ConfigReadmeDto
        {
            MarkdownText = config.Readme
        };
    }

    /// <summary>
    /// Get a config's data.
    /// </summary>
    [Admin]
    [HttpGet]
    [MapToApiVersion("1.0")]
    public ActionResult<ConfigDto> GetConfig(string id)
    {
        var config = GetConfigFromService(id);
        return _mapper.Map<ConfigDto>(config);
    }

    /// <summary>
    /// Update a config's data.
    /// </summary>
    [Admin]
    [HttpPut]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ConfigDto>> UpdateConfig(ConfigDto dto)
    {
        // Make sure a config with this id exists
        var config = GetConfigFromService(dto.Id);

        // Make sure it's not a remote config
        if (config.IsRemote)
        {
            throw new ActionNotAllowedException(
                ErrorCode.CANNOT_EDIT_REMOTE_CONFIG,
                $"Attempted to edit remote config with id {dto.Id}");
        }

        // Apply the new fields to the EXISTING config
        _mapper.Map(dto, config);

        // Save it
        await _configRepo.Save(config);

        return _mapper.Map<ConfigDto>(config);
    }

    private Config GetConfigFromService(string id)
    {
        var config = _configService.Configs.FirstOrDefault(c => c.Id == id);

        if (config is null)
        {
            throw new EntryNotFoundException(ErrorCode.CONFIG_NOT_FOUND,
                id, nameof(ConfigService));
        }

        return config;
    }
}
