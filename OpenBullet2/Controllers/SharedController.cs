using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Services;
using System.Threading.Tasks;

namespace OpenBullet2.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SharedController : Controller
    {
        private readonly ConfigSharingService configSharingService;

        public SharedController(ConfigSharingService configSharingService)
        {
            this.configSharingService = configSharingService;
        }

        [HttpGet("configs/{endpoint}")]
        public async Task<IActionResult> DownloadConfigs(string endpoint)
        {
            try
            {
                return File(await configSharingService.GetArchive(endpoint), "application/octet-stream", $"Configs.zip");
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
