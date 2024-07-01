using OpenBullet2.Web.Models.Errors;

namespace OpenBullet2.Web.Tests.Utils;

public class ApiErrorResponse
{
    public ApiError? Content { get; set; }
    public required HttpResponseMessage Response { get; set; }
}
