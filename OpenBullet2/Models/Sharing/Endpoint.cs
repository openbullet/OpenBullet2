using System.Collections.Generic;

namespace OpenBullet2.Models.Sharing
{
    public class Endpoint
    {
        public string Route = "configs";
        public List<string> ApiKeys = new();
        public List<string> ConfigIds = new();
    }
}
