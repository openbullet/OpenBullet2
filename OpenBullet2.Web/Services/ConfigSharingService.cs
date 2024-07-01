using Newtonsoft.Json;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Exceptions;
using RuriLib.Helpers;
using RuriLib.Models.Configs;
using System.IO.Compression;
using Endpoint = OpenBullet2.Core.Models.Sharing.Endpoint;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Service used to share configs with other users via API.
/// </summary>
public class ConfigSharingService
{
    private readonly IConfigRepository _configRepo;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly ILogger<ConfigSharingService> _logger;

    /// <summary></summary>
    public ConfigSharingService(IConfigRepository configRepo,
        ILogger<ConfigSharingService> logger, string settingsFolder)
    {
        _logger = logger;
        _configRepo = configRepo;
        SettingsFolder = settingsFolder;
        Directory.CreateDirectory(settingsFolder);

        _jsonSettings =
            new JsonSerializerSettings { Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.Auto };

        if (File.Exists(EndpointsFile))
        {
            Endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(
                File.ReadAllText(EndpointsFile), _jsonSettings)!;
        }
    }

    private string SettingsFolder { get; }
    private string EndpointsFile => Path.Combine(SettingsFolder, "sharingEndpoints.json");

    /// <summary>
    /// The configured shared endpoints.
    /// </summary>
    public List<Endpoint> Endpoints { get; set; } = new();

    /// <summary>
    /// Saves the endpoints configuration to file.
    /// </summary>
    public void Save() => File.WriteAllText(
        EndpointsFile, JsonConvert.SerializeObject(Endpoints, _jsonSettings));

    /// <summary>
    /// Gets an endpoint by name, returns null if not found.
    /// </summary>
    public Endpoint? GetEndpoint(string endpointName)
        => Endpoints.Find(e => e.Route.Equals(endpointName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the bytes of the zip archive containing the configs
    /// that need to be shared via the given endpoint.
    /// </summary>
    public async Task<byte[]> GetArchiveAsync(string endpointName)
    {
        var endpoint = GetEndpoint(endpointName);

        if (endpoint == null)
        {
            throw new EntryNotFoundException(ErrorCode.EndpointNotFound, "Invalid endpoint");
        }

        using MemoryStream ms = new();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var configId in endpoint.ConfigIds)
            {
                try
                {
                    // Get the config from the repo, randomize the id, convert it to C# and repack it into bytes
                    var config = await _configRepo.GetAsync(configId);
                    config.ChangeMode(ConfigMode.CSharp);
                    config.Id = Guid.NewGuid().ToString();
                    var bytes = await ConfigPacker.PackAsync(config);

                    // Create the entry and write the data
                    var zipArchiveEntry = archive.CreateEntry($"{configId}.opk", CompressionLevel.Fastest);
                    await using var zipStream = zipArchiveEntry.Open();
                    await zipStream.WriteAsync(bytes);
                }
                catch (Exception ex)
                {
                    // If something happens, simply log it and omit the config from the archive
                    _logger.LogError(ex, "Error while packing config {ConfigId} for endpoint {Route}: {Message}",
                        configId, endpoint.Route, ex.Message);
                }
            }
        }

        return ms.ToArray();
    }
}
