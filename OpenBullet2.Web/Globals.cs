using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenBullet2.Web;

internal static class Globals
{
    /// <summary>
    /// When the server was started.
    /// </summary>
    public static DateTime StartTime { get; set; }

    /// <summary>
    /// An updated Win11 + Chrome user-agent to use for http calls.
    /// </summary>
    public static string UserAgent => "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };
}
