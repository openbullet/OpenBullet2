using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Info;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using RuriLib.Services;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Get info about the server.
/// </summary>
[TypeFilter<GuestFilter>]
[ApiVersion("1.0")]
public class InfoController : ApiController
{
    private readonly IAnnouncementService _announcementService;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUpdateService _updateService;

    /// <summary></summary>
    public InfoController(IAnnouncementService announcementService,
        HttpClient httpClient,
        IUpdateService updateService, IServiceProvider serviceProvider)
    {
        _announcementService = announcementService;
        _httpClient = httpClient;
        _updateService = updateService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Get information about the server and the environment.
    /// </summary>
    [HttpGet("server")]
    [MapToApiVersion("1.0")]
    public ActionResult<ServerInfoDto> GetServerInfo()
    {
        var version = Assembly
            .GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 1);

        var buildDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddDays(version.Build).AddSeconds(version.Revision * 2);

        var buildNumber = $"{version.Build}.{version.Revision}";

        var ip = HttpContext.Connection?.RemoteIpAddress
                 ?? IPAddress.Parse("127.0.0.1");

        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        return new ServerInfoDto {
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
        var markdown = await _announcementService.FetchAnnouncementAsync();

        return new AnnouncementDto { MarkdownText = markdown, LastFetched = _announcementService.LastFetched };
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
        string markdown;

        // If a version was not provided, use the current version
        v ??= _updateService.CurrentVersion.ToString();

        try
        {
            // The changelog is only present for stable builds in the master branch
            var url = $"https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Changelog/{v}.md";
            using var response = await _httpClient.GetAsync(url);

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

        return new ChangelogDto { MarkdownText = markdown, Version = v };
    }

    /// <summary>
    /// Get information about the availability of a new update.
    /// </summary>
    [HttpGet("update")]
    [MapToApiVersion("1.0")]
    public ActionResult<UpdateInfoDto> GetUpdateInfo() => new UpdateInfoDto {
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
        var apiUser = HttpContext.GetApiUser();
        
        var jobCount = apiUser.Role is UserRole.Admin
            ? _serviceProvider.GetRequiredService<JobManagerService>()
                .Jobs.Count()
            : _serviceProvider.GetRequiredService<JobManagerService>()
                .Jobs.Count(j => j.OwnerId == apiUser.Id);

        var proxyCount = apiUser.Role is UserRole.Admin
            ? await _serviceProvider.GetRequiredService<IProxyRepository>()
                .GetAll().CountAsync()
            : await _serviceProvider.GetRequiredService<IProxyRepository>()
                .GetAll().CountAsync(p => p.Group.Owner.Id == apiUser.Id);

        var wordlistCount = apiUser.Role is UserRole.Admin
            ? await _serviceProvider.GetRequiredService<IWordlistRepository>()
                .GetAll().CountAsync()
            : await _serviceProvider.GetRequiredService<IWordlistRepository>()
                .GetAll().CountAsync(w => w.Owner.Id == apiUser.Id);

        var wordlistLines = apiUser.Role is UserRole.Admin
            ? await _serviceProvider.GetRequiredService<IWordlistRepository>()
                .GetAll().SumAsync(w => (long)w.Total)
            : await _serviceProvider.GetRequiredService<IWordlistRepository>()
                .GetAll().Where(w => w.Owner.Id == apiUser.Id).SumAsync(w => (long)w.Total);

        var hitCount = apiUser.Role is UserRole.Admin
            ? await _serviceProvider.GetRequiredService<IHitRepository>()
                .GetAll().CountAsync()
            : await _serviceProvider.GetRequiredService<IHitRepository>()
                .GetAll().CountAsync(h => h.OwnerId == apiUser.Id);

        var configCount = _serviceProvider
            .GetRequiredService<ConfigService>().Configs.Count;

        var guestCount = apiUser.Role is UserRole.Admin 
            ? await _serviceProvider.GetRequiredService<IGuestRepository>().GetAll().CountAsync()
            : 1; // The guest shouldn't see the total number of guests

        var pluginCount = apiUser.Role is UserRole.Admin
            ? _serviceProvider.GetRequiredService<PluginRepository>().GetPlugins().Count()
            : 0; // The guest shouldn't see the total number of plugins

        return new CollectionInfoDto {
            JobsCount = jobCount,
            ProxiesCount = proxyCount,
            WordlistsCount = wordlistCount,
            WordlistsLines = wordlistLines,
            HitsCount = hitCount,
            ConfigsCount = configCount,
            GuestsCount = guestCount,
            PluginsCount = pluginCount
        };
    }

    private static string GetOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.OSVersion.VersionString;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"macOS {Environment.OSVersion.Version}";
        }

        return RuntimeInformation.OSDescription;
    }
}
