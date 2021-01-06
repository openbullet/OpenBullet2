using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public interface IThemeRepository
    {
        Task AddFromCss(string name, string css);
        Task AddFromCssFile(string fileName, Stream stream);
        Task AddFromZipArchive(Stream stream);
        Task<IEnumerable<string>> GetNames();
        Task<string> GetPath(string name);
    }
}
