using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class DatabaseController : Controller
    {
        [HttpGet("download")]
        public async Task<IActionResult> DownloadDatabase()
        {
            try
            {
                var bytes = await System.IO.File.ReadAllBytesAsync("OpenBullet.db");
                var stream = new MemoryStream(bytes);

                return File(stream, "application/octet-stream", $"OpenBullet.db");
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
