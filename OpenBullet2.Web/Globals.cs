using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenBullet2.Web;

internal static class Globals
{
    public static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// When the server was started.
    /// </summary>
    public static DateTime StartTime { get; set; }
    
    /// <summary>
    /// The folder where user data is stored.
    /// </summary>
    public static string UserDataFolder { get; set; } = "UserData";

    /// <summary>
    /// An updated Win11 + Chrome user-agent to use for http calls.
    /// </summary>
    public static string UserAgent =>
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";
}
