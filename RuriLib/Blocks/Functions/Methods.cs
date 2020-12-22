using PluginFramework.Attributes;
using RuriLib.Models.Bots;
using RuriLib.Models.UserAgents;

namespace RuriLib.Blocks.Functions
{
    [BlockCategory("Functions", "General purpose functions", "#9acd32")]
    public static class Methods
    {
        [Block("Generates a random User Agent using the builtin provider or a custom list")]
        public static string RandomUserAgent(BotData data, UAPlatform platform = UAPlatform.All)
        {
            data.Logger.LogHeader();
            string ua;

            if (data.GlobalSettings.GeneralSettings.UseCustomUserAgentsList)
            {
                data.Logger.Log("Getting random UA from custom list");
                var userAgents = data.GlobalSettings.GeneralSettings.UserAgents;

                if (userAgents.Count == 0)
                    ua = "NO_RANDOM_UA_FOUND";

                else
                    ua = userAgents[data.Random.Next(userAgents.Count)];
            }
            else
            {
                data.Logger.Log("Getting random UA from the builtin provider");

                if (data.RandomUAProvider == null)
                    ua = "NO_RANDOM_UA_PROVIDER_SPECIFIED";

                else if (data.RandomUAProvider.Total == 0)
                    ua = "NO_RANDOM_UA_FOUND";

                else
                    ua = data.RandomUAProvider.Generate(platform);
            }

            data.Logger.Log(ua);
            return ua;
        }
    }
}
