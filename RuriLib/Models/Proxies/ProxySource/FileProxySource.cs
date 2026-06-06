using RuriLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies.ProxySources;

/// <summary>
/// Loads proxies from a file or executable script.
/// </summary>
public class FileProxySource : ProxySource
{
    /// <summary>
    /// Gets or sets the file or script path.
    /// </summary>
    public string FileName { get; set; }
    private AsyncLocker? asyncLocker;

    /// <summary>
    /// Creates a file-backed proxy source.
    /// </summary>
    /// <param name="fileName">The file or script path.</param>
    public FileProxySource(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        FileName = fileName;
        asyncLocker = new();
    }

    /// <inheritdoc />
    public async override Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        string[] lines;
        var supportedScripts = new[] { ".bat", ".ps1", ".sh" };
        var fileExtension = (Path.GetExtension(FileName) ?? string.Empty).ToLowerInvariant();
        if (fileExtension.Length != 0 && supportedScripts.Contains(fileExtension))
        {
            if (UserId > 0)
            {
                throw new UnauthorizedAccessException(
                    "Script-based proxy sources are not allowed for guest users");
            }

            var locker = asyncLocker ?? throw new ObjectDisposedException(nameof(FileProxySource));

            // The file is a script.
            // We will run the execute and read it's stdout for proxies.
            // just like raw proxy files, one proxy per line
            await locker.Acquire("ProxySourceReloadScriptFile", CancellationToken.None).ConfigureAwait(false);
            try
            {
                var stdout = await RunScript.RunScriptAndGetStdOut(FileName).ConfigureAwait(false);
                if (stdout is null)
                {
                    throw new Exception($"Failed to get stdout of {FileName}");
                }

                lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            }
            finally
            {
                locker.Release("ProxySourceReloadScriptFile");
            }
        }
        else
        {
            lines = await File.ReadAllLinesAsync(FileName, cancellationToken).ConfigureAwait(false);
        }

        return lines
            .Select(l => Proxy.TryParse(l.Trim(), out var proxy, DefaultType, DefaultUsername, DefaultPassword) ? proxy : null)
            .OfType<Proxy>();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if (asyncLocker is not null)
        {
            try
            {
                asyncLocker.Dispose();
                asyncLocker = null;
            }
            catch
            {
                // ignored
            }
        }
    }
}
