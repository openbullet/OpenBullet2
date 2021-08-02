using System.Collections.Generic;

namespace OpenBullet2.Core.Models.Settings
{
    /// <summary>
    /// A remote endpoint that hosts configs.
    /// </summary>
    public class RemoteConfigsEndpoint
    {
        /// <summary>
        /// The URL of the endpoint.
        /// </summary>
        public string Url { get; set; } = "http://x.x.x.x:5000/api/shared/configs/ENDPOINT_NAME";

        /// <summary>
        /// The API key to use to access the endpoint.
        /// </summary>
        public string ApiKey { get; set; } = "MY_API_KEY";
    }

    /// <summary>
    /// Settings related to remote endpoints.
    /// </summary>
    public class RemoteSettings
    {
        /// <summary>
        /// Remote endpoints from which configs will be fetched by the config manager
        /// upon reload.
        /// </summary>
        public List<RemoteConfigsEndpoint> ConfigsEndpoints { get; set; } = new();
    }
}
