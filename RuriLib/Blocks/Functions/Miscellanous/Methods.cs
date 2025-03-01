using RuriLib.Attributes;
using RuriLib.Models.Bots;
using RuriLib.Providers.UserAgents;
using System;

namespace RuriLib.Blocks.Functions.Miscellanous
{
    [BlockCategory("Miscellanous", "General purpose functions", "#9acd32")]
    public static class Methods
    {
        [Block("Generates a random User Agent using the builtin provider or a custom list")]
        public static string RandomUserAgent(BotData data, UAPlatform platform = UAPlatform.All)
        {
            data.Logger.LogHeader();
            data.Logger.Log("Getting random UA from the builtin provider");
            string ua;

            try
            {
                ua = data.Providers.RandomUA.Generate(platform);
            }
            catch
            {
                ua = "NO_RANDOM_UA_FOUND";
            }

            data.Logger.Log(ua);
            return ua;
        }

        [Block("Generates a random GUID using the builtin provider",
            extraInfo = "Full list of formats here: https://learn.microsoft.com/en-us/dotnet/api/system.guid.tostring under the 'Remarks' section")]
        public static string RandomGuid(BotData data, string format = "D")
        {
            data.Logger.LogHeader();
            data.Logger.Log("Getting random GUID from the builtin provider");
            
            var guid = Guid.NewGuid().ToString(format);
            data.Logger.Log(guid);

            return guid;
        }
    }
}
