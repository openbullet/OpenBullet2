using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Extensions;

static internal class HttpContextExtensions
{
    public static ApiUser GetApiUser(this HttpContext context)
        => (ApiUser)(context.Items["apiUser"] ??
        throw new UnauthorizedAccessException("Auth token not provided"));

    public static void SetApiUser(this HttpContext context, ApiUser value)
        => context.Items["apiUser"] = value;
}
