using PluginFramework.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System.Net;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Utility
{
    [BlockCategory("Utility", "Utility blocks for miscellaneous purposes", "#fad6a5")]
    public static class Methods
    {
        [Block("Clears the cookie jar used for HTTP requests")]
        public static void ClearCookies(BotData data)
        {
            data.CookieContainer = new CookieContainer();
            data.Logger.LogHeader();
            data.Logger.Log($"Cleared the HTTP cookie jar", LogColors.DeepChampagne);
        }

        [Block("Sleeps for a specified amount of milliseconds")]
        public static async Task Delay(BotData data, int milliseconds)
        {
            data.Logger.LogHeader();
            await Task.Delay(milliseconds);
            data.Logger.Log($"Waited {milliseconds} ms");
        }
    }
}
