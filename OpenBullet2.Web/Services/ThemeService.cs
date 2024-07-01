namespace OpenBullet2.Web.Services;

/// <summary>
/// Service to manage themes.
/// </summary>
public class ThemeService
{
    private readonly string _basePath;

    /// <summary></summary>
    public ThemeService(string basePath)
    {
        _basePath = basePath;

        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Saves a CSS file.
    /// </summary>
    /// <param name="fileName">Must end in .css</param>
    /// <param name="stream">The file stream</param>
    public async Task SaveCssFileAsync(string fileName, Stream stream)
    {
        if (!fileName.EndsWith(".css"))
        {
            throw new ArgumentException("File name must end with .css");
        }

        var path = Path.Combine(_basePath, fileName);

        await using var fs = new FileStream(path, FileMode.Create);
        await stream.CopyToAsync(fs);
    }

    /// <summary>
    /// Get all theme names.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetThemeNames() =>
        Directory.GetFiles(_basePath)
            .Where(f => f.EndsWith(".css"))
            .Select(f => Path.GetFileNameWithoutExtension(f).Replace(".css", ""));

    /// <summary>
    /// Gets a CSS file by name.
    /// </summary>
    /// <param name="name">The name of the file</param>
    public async Task<byte[]> GetCssFileAsync(string name)
    {
        var fileName = name + ".css";

        var path = Path.Combine(_basePath, fileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Theme file {fileName} not found.");
        }

        return await File.ReadAllBytesAsync(path);
    }
}
