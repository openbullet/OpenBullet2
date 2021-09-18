using DeviceId;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Utility
{
    [BlockCategory("Utility", "Utility blocks for miscellaneous purposes", "#fad6a5")]
    public static class Methods
    {
        [Block("Clears the cookie jar used for HTTP requests")]
        public static void ClearCookies(BotData data)
        {
            data.COOKIES = new();
            data.Logger.LogHeader();
            data.Logger.Log($"Cleared the HTTP cookie jar", LogColors.DeepChampagne);
        }

        [Block("Sleeps for a specified amount of milliseconds")]
        public static async Task Delay(BotData data, int milliseconds)
        {
            data.Logger.LogHeader();
            await Task.Delay(milliseconds, data.CancellationToken).ConfigureAwait(false);
            data.Logger.Log($"Waited {milliseconds} ms", LogColors.DeepChampagne);
        }

        [Block("Retrieves a unique hardware ID for the current machine", name = "Get HWID")]
        public static string GetHWID(BotData data)
        {
            var builder = new DeviceIdBuilder()
                .AddUserName()
                .AddMachineName()
                .AddOSVersion()
                .AddMacAddress()
                .AddSystemDriveSerialNumber()
                .AddOSInstallationID();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder
                    .AddProcessorId()
                    .AddMotherboardSerialNumber()
                    .AddSystemUUID();
            }

            var hwid = builder.ToString();

            data.Logger.LogHeader();
            data.Logger.Log($"Got HWID {hwid}", LogColors.DeepChampagne);
            return hwid;
        }
    }
}
