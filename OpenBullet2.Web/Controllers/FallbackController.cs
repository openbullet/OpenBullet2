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
    public ActionResult Index()
    {
        var indexPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "index.html");

        return System.IO.File.Exists(indexPath)
            ? PhysicalFile(indexPath, "text/HTML")
            : NotFound();
    }
}
