using OpenBullet2.Models.Keycheck;
using System.Collections.Generic;

namespace OpenBullet2.Models
{
    public class KeycheckBlock : BlockInstance
    {
        public List<Keychain> Keychains { get; set; } = new List<Keychain>();
    }
}
