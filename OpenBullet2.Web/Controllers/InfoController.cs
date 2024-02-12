using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Info;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Services;
using RuriLib.Services;
using System.Net;
using System.Runtime.InteropServices;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Get info about the server.
/// </summary>
[Guest]
[ApiVersion("1.0")]
public class InfoController : ApiController
{
    private readonly IAnnouncementService _announcementService;
    private readonly IUpdateService _updateService;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;
    private readonly PerformanceMonitorService _performanceMonitorService;

    /// <summary></summary>
    public InfoController(IAnnouncementService announcementService,
        IUpdateService updateService, IMapper mapper,
        IServiceProvider serviceProvider,
        PerformanceMonitorService performanceMonitorService)
    {
        _announcementService = announcementService;
        _updateService = updateService;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
        _performanceMonitorService = performanceMonitorService;
    }

    /// <summary>
    /// Get information about the server and the environment.
    /// </summary>
    [HttpGet("server")]
    [MapToApiVersion("1.0")]
    public ActionResult<ServerInfoDto> GetServerInfo()
    {
        var version = System.Reflection.Assembly
            .GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 1);

        var buildDate = new DateTime(2000, 1, 1)
            .AddDays(version.Build).AddSeconds(version.Revision * 2);
        
        var buildNumber = $"{version.Build}.{version.Revision}";

        var ip = HttpContext.Connection?.RemoteIpAddress
            ?? IPAddress.Parse("127.0.0.1");

        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        return new ServerInfoDto
        {
            LocalUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now),
            StartTime = Globals.StartTime,
            OperatingSystem = GetOperatingSystem(),
            CurrentWorkingDirectory = Directory.GetCurrentDirectory(),
            BuildNumber = buildNumber,
            BuildDate = buildDate,
            ClientIpAddress = ip.ToString()
        };
    }

    /// <summary>
    /// Get the current announcement.
    /// </summary>
    [HttpGet("announcement")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AnnouncementDto>> GetAnnouncement()
    {
        var markdown = await _announcementService.FetchAnnouncement();

        return new AnnouncementDto
        {
            MarkdownText = markdown,
            LastFetched = _announcementService.LastFetched
        };
    }

    /// <summary>
    /// Get the changelog for a given version of the software.
    /// If no version is specified, the current version will be used.
    /// </summary>
    [HttpGet("changelog")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ChangelogDto>> GetChangelog(string? v)
    {
        // NOTE: We cannot call the query param "version" otherwise ASP.NET core
        // will set its value to the API version instead of what was passed :|
        var markdown = string.Empty;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");

        // If a version was not provided, use the current version
        v ??= _updateService.CurrentVersion.ToString();

        try
        {
            var url = $"https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Changelog/{v}.md";
            using var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ResourceNotFoundException(
                    ErrorCode.RemoteResourceNotFound,
                    $"Changelog for version {v}", url);
            }

            markdown = await response.Content.ReadAsStringAsync();
        }
        catch (ResourceNotFoundException)
        {
            // Rethrow this since it is handled by the middleware
            throw;
        }
        catch
        {
            markdown = "Could not retrieve the changelog";
        }

        return new ChangelogDto 
        {
            MarkdownText = markdown,
            Version = v
        };
    }

    /// <summary>
    /// Get information about the availability of a new update.
    /// </summary>
    [HttpGet("update")]
    [MapToApiVersion("1.0")]
    public ActionResult<UpdateInfoDto> GetUpdateInfo() => new UpdateInfoDto
    {
        CurrentVersion = _updateService.CurrentVersion.ToString(),
        RemoteVersion = _updateService.RemoteVersion.ToString(),
        IsUpdateAvailable = _updateService.IsUpdateAvailable,
        CurrentVersionType = _updateService.CurrentVersionType,
        RemoteVersionType = _updateService.RemoteVersionType
    };

    /// <summary>
    /// Get information about your collection of wordlists, proxies etc.
    /// </summary>
    [HttpGet("collection")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<CollectionInfoDto>> GetCollectionInfo()
    {
        var jobsCount = _serviceProvider
            .GetRequiredService<JobManagerService>().Jobs.Count();

        var proxiesCount = await _serviceProvider
            .GetRequiredService<IProxyRepository>().GetAll().CountAsync();

        var wordlistsCount = await _serviceProvider
            .GetRequiredService<IWordlistRepository>().GetAll().CountAsync();
        
        var wordlistsLines = await _serviceProvider
            .GetRequiredService<IWordlistRepository>().GetAll().SumAsync(w => (long)w.Total);

        var hitsCount = await _serviceProvider
            .GetRequiredService<IHitRepository>().GetAll().CountAsync();

        var configsCount = _serviceProvider
            .GetRequiredService<ConfigService>().Configs.Count;

        var guestsCount = await _serviceProvider
            .GetRequiredService<IGuestRepository>().GetAll().CountAsync();

        var pluginsCount = _serviceProvider
            .GetRequiredService<PluginRepository>().GetPlugins().Count();

        return new CollectionInfoDto
        {
            JobsCount = jobsCount,
            ProxiesCount = proxiesCount,
            WordlistsCount = wordlistsCount,
            WordlistsLines = wordlistsLines,
            HitsCount = hitsCount,
            ConfigsCount = configsCount,
            GuestsCount = guestsCount,
            PluginsCount = pluginsCount
        };
    }

    private static string GetOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Environment.OSVersion.VersionString;

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return $"macOS {Environment.OSVersion.Version}";

        else
            return RuntimeInformation.OSDescription;
    }
}
