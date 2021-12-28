using System.Collections.Generic;

namespace OpenBullet2.Core.Models.Sharing
{
    /// <summary>
    /// A sharing endpoint that will be used to share configs with other
    /// OpenBullet 2 instances.
    /// </summary>
    public class Endpoint
    {
        /// <summary>
        /// The route for the endpoint.
        /// </summary>
        public string Route { get; set; } = "configs";

        /// <summary>
        /// The API keys that are allowed to access the endpoint. When requesting configs
        /// from this endpoint, users will send their API key inside the HTTP request.
        /// </summary>
        public List<string> ApiKeys { get; set; } = new();

        /// <summary>
        /// The IDs of the configs that will be delivered by the server to the clients.
        /// </summary>
        public List<string> ConfigIds { get; set; } = new();
    }
}
