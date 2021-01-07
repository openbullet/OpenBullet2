using System.Collections.Generic;

namespace OpenBullet2.Models.Settings
{
    public class RemoteConfigsEndpoint
    {
        public string Url { get; set; } = "http://x.x.x.x:5000/api/shared/configs/ENDPOINT_NAME";
        public string ApiKey { get; set; } = "MY_API_KEY";
    }

    public class RemoteSettings
    {
        public List<RemoteConfigsEndpoint> ConfigsEndpoints { get; set; } = new();
    }
}
