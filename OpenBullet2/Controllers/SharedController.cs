using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Services;
using System.Linq;
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

        [HttpGet("configs/{endpointName}")]
        public async Task<IActionResult> DownloadConfigs(string endpointName)
        {
            try
            {
                var apiKey = Request.Headers["Api-Key"].First();
                var endpoint = configSharingService.GetEndpoint(endpointName);

                if (!endpoint.ApiKeys.Contains(apiKey))
                {
                    return Unauthorized();
                }

                return File(await configSharingService.GetArchive(endpointName), "application/octet-stream", $"Configs.zip");
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
