using RuriLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies.ProxySources
{
    public class FileProxySource : ProxySource
    {
        public string FileName { get; set; }
        private AsyncLocker asyncLocker;

        public FileProxySource(string fileName)
        {
            FileName = fileName;
            asyncLocker = new();
        }

        public async override Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            string[] lines;
            var supportedScripts = new[] { ".bat", ".ps1", ".sh" };
            var fileExtension = (Path.GetExtension(FileName) ?? "").ToLower();
            if (fileExtension.Length != 0 && supportedScripts.Contains(fileExtension))
            {
                // The file is a script.
                // We will run the execute and read it's stdout for proxies.
                // just like raw proxy files, one proxy per line
                await asyncLocker.Acquire("ProxySourceReloadScriptFile", CancellationToken.None).ConfigureAwait(false);
                var stdout = await RunScript.RunScriptAndGetStdOut(FileName).ConfigureAwait(false);
                if (stdout is null)
                {
                    throw new Exception($"Failed to get stdout of {FileName}");
                }
                lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                asyncLocker.Release("ProxySourceReloadScriptFile");
            }
            else
            {
                lines = await File.ReadAllLinesAsync(FileName, cancellationToken);
            }

            return lines
                .Select(l => Proxy.TryParse(l.Trim(), out var proxy, DefaultType, DefaultUsername, DefaultPassword) ? proxy : null)
                .Where(p => p != null);
        }

        public override void Dispose()
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
