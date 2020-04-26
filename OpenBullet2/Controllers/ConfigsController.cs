using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Repositories;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class ConfigsController : Controller
    {
        private readonly IConfigRepository repo;

        public ConfigsController(IConfigRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadConfig(string id)
        {
            try
            {
                var config = await repo.Get(id);

                // Make a valid filename
                string fileName = config.Metadata.Name;
                foreach (char c in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(c, '_');

                var bytes = await System.IO.File.ReadAllBytesAsync($"Configs/{id}.opk");
                var stream = new MemoryStream(bytes);
                
                return File(stream, "application/octet-stream", $"{fileName}.opk");
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
