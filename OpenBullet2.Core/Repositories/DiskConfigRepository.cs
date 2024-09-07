using RuriLib.Models.Configs;
using RuriLib.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using RuriLib.Helpers.Transpilers;
using RuriLib.Services;
using RuriLib.Legacy.Configs;
using System.Text;
using OpenBullet2.Core.Exceptions;

namespace OpenBullet2.Core.Repositories;

/// <summary>
/// Stores configs on disk.
/// </summary>
public class DiskConfigRepository : IConfigRepository
{
    private readonly RuriLibSettingsService _rlSettings;

    private string BaseFolder { get; init; }

    public DiskConfigRepository(RuriLibSettingsService rlSettings, string baseFolder)
    {
        _rlSettings = rlSettings;
        BaseFolder = baseFolder;
        Directory.CreateDirectory(baseFolder);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Config>> GetAllAsync()
    {
        // Try to convert legacy configs automatically before loading
        foreach (var file in Directory.GetFiles(BaseFolder).Where(file => file.EndsWith(".loli")))
        {
            try
            {
                var id = Path.GetFileNameWithoutExtension(file);
                var converted = ConfigConverter.Convert(File.ReadAllText(file), id);
                await SaveAsync(converted);
                File.Delete(file);
                Console.WriteLine($"Converted legacy .loli config ({file}) to the new .opk format");
            }
            catch
            {
                Console.WriteLine($"Could not convert legacy .loli config ({file}) to the new .opk format");
            }
        }

        var tasks = Directory.GetFiles(BaseFolder).Where(file => file.EndsWith(".opk"))
            .Select(async file => 
            {
                try
                {
                    return await GetAsync(Path.GetFileNameWithoutExtension(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not unpack {file} properly: {ex.Message}");
                    return null;
                }
            });

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null);
    }

    /// <inheritdoc/>
    public async Task<Config> GetAsync(string id)
    {
        var file = GetFileName(id);

        if (!File.Exists(file))
        {
            throw new FileNotFoundException();
        }
        
        await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

        var config = await ConfigPacker.UnpackAsync(fileStream);
        config.Id = id;
        return config;

    }

    /// <inheritdoc/>
    public async Task<byte[]> GetBytesAsync(string id)
    {
        var file = GetFileName(id);

        if (!File.Exists(file))
        {
            throw new FileNotFoundException();
        }
        
        await using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read);
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);

        return ms.ToArray();

    }

    /// <inheritdoc/>
    public async Task<Config> CreateAsync(string id = null)
    {
        var config = new Config { Id = id ?? Guid.NewGuid().ToString() };

        config.Settings.DataSettings.AllowedWordlistTypes = [
            _rlSettings.Environment.WordlistTypes.First().Name
        ];

        await SaveAsync(config);
        return config;
    }

    /// <inheritdoc/>
    public async Task UploadAsync(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName);

        // If it's a .opk config
        if (extension == ".opk")
        {
            var config = await ConfigPacker.UnpackAsync(stream);
            await File.WriteAllBytesAsync(GetFileName(config), await ConfigPacker.PackAsync(config));
        }
        // Otherwise it's a .loli config
        else if (extension == ".loli")
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var content = Encoding.UTF8.GetString(ms.ToArray());
            var id = Path.GetFileNameWithoutExtension(fileName);
            var converted = ConfigConverter.Convert(content, id);
            await SaveAsync(converted);
        }
        else
        {
            throw new UnsupportedFileTypeException($"Unsupported file type: {extension}");
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync(Config config)
    {
        // Update the last modified date
        config.Metadata.LastModified = DateTime.Now;

        // If it's possible to retrieve the block descriptors, get required plugins
        if (config.Mode is ConfigMode.Stack or ConfigMode.LoliCode)
        {
            try
            {
                var stack = config.Mode is ConfigMode.Stack
                    ? config.Stack
                    : Loli2StackTranspiler.Transpile(config.LoliCodeScript);

                // Write the required plugins in the config's metadata
                config.Metadata.Plugins = stack.Select(b => b.Descriptor.AssemblyFullName)
                    .Where(n => n != null && !n.Contains("RuriLib")).ToList();
            }
            catch
            {
                // Don't do anything, it's not the end of the world if we don't write some metadata ^_^
            }
        }

        await File.WriteAllBytesAsync(GetFileName(config), await ConfigPacker.PackAsync(config));
    }

    /// <inheritdoc/>
    public void Delete(Config config)
    {
        var file = GetFileName(config);

        if (File.Exists(file))
            File.Delete(file);
    }

    private string GetFileName(Config config)
        => GetFileName(config.Id);

    private string GetFileName(string id)
        => Path.Combine(BaseFolder, $"{id}.opk").Replace('\\', '/');
}
