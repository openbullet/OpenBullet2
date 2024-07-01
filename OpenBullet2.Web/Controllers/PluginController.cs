using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Dtos.Plugin;
using OpenBullet2.Web.Exceptions;
using RuriLib.Services;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage plugins.
/// </summary>
[TypeFilter<AdminFilter>]
[ApiVersion("1.0")]
public class PluginController : ApiController
{
    private readonly PluginRepository _pluginRepository;
    private readonly ILogger<PluginController> _logger;
    
    /// <summary></summary>
    public PluginController(PluginRepository pluginRepository,
        ILogger<PluginController> logger)
    {
        _pluginRepository = pluginRepository;
        _logger = logger;
    }

    /// <summary>
    /// List all active plugins.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<PluginDto>> GetAll()
    {
        var pluginNames = _pluginRepository.GetPluginNames();
        var plugins = pluginNames.Select(p => new PluginDto { Name = p });
        return Ok(plugins);
    }

    /// <summary>
    /// Mark a plugin to be deleted when the server is restarted.
    /// </summary>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public ActionResult Delete(string name)
    {
        var pluginNames = _pluginRepository.GetPluginNames();

        if (!pluginNames.Contains(name))
        {
            throw new EntryNotFoundException(
                ErrorCode.PluginNotFound, name, nameof(PluginRepository));
        }

        _pluginRepository.DeletePlugin(name);
        
        _logger.LogInformation(
            "Plugin {PluginName} marked for deletion, will be deleted upon server restart",
            name);
        
        return Ok();
    }

    /// <summary>
    /// Add a new plugin from a zip archive.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public ActionResult Add(IFormFile file)
    {
        _pluginRepository.AddPlugin(file.OpenReadStream());
        
        _logger.LogInformation("Plugin added from file {FileName}", file.FileName);
        
        return Ok();
    }
}
