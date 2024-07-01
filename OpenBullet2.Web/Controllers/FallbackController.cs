using Microsoft.AspNetCore.Mvc;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Default controller when no path is matched.
/// </summary>
public class FallbackController : Controller
{
    /// <summary>
    /// Get the index.html page.
    /// </summary>
    public ActionResult Index() => PhysicalFile(Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "index.html"), "text/HTML");
}
