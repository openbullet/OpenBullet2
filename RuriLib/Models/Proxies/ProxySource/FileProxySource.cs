using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies.ProxySources
{
    public class FileProxySource : ProxySource
    {
        public string FileName { get; set; } = string.Empty;

        public FileProxySource(string fileName)
        {
            FileName = fileName;
        }

        public override async Task<IEnumerable<Proxy>> GetAll()
        {
            var lines = await File.ReadAllLinesAsync(FileName);
            return lines.Select(l => Proxy.Parse(l, DefaultType, DefaultUsername, DefaultPassword));
        }
    }
}
