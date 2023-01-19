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
        var config = GetConfig(id);

        return new ConfigReadmeDto
        {
            MarkdownText = config.Readme
        };
    }

    /// <summary>
    /// Update the readme of a config.
    /// </summary>
    [Admin]
    [HttpPut("readme")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ConfigReadmeDto>> UpdateReadme(
        UpdateConfigReadmeDto dto)
    {
        var config = GetConfig(dto.Id);
        config.Readme = dto.MarkdownText;
        await _configRepo.Save(config);

        _logger.LogInformation($"Updated the readme of the config {config.Metadata.Name}");

        return new ConfigReadmeDto
        {
            MarkdownText = config.Readme
        };
    }

    /// <summary>
    /// Get the metadata of a config.
    /// </summary>
    [Admin]
    [HttpGet("metadata")]
    [MapToApiVersion("1.0")]
    public ActionResult<ConfigMetadataDto> GetMetadata(string id)
    {
        var config = GetConfig(id);
        return _mapper.Map<ConfigMetadataDto>(config.Metadata);
    }

    /// <summary>
    /// Update the metadata of a config.
    /// </summary>
    [Admin]
    [HttpPut("metadata")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ConfigMetadataDto>> UpdateMetadata(
        UpdateConfigMetadataDto dto)
    {
        var config = GetConfig(dto.Id);
        config.Metadata = _mapper.Map<ConfigMetadata>(dto);
        await _configRepo.Save(config);

        _logger.LogInformation($"Updated the metadata of the config {config.Metadata.Name}");

        return _mapper.Map<ConfigMetadataDto>(config.Metadata);
    }

    private Config GetConfig(string id)
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
