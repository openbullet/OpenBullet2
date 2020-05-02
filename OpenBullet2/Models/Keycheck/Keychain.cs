using OpenBullet2.Enums;
using System.Collections.Generic;

namespace OpenBullet2.Models.Keycheck
{
    public class Keychain
    {
        public List<Key> Keys { get; set; } = new List<Key>();
        public KeychainMode Mode { get; set; } = KeychainMode.OR;
        public string ResultStatus { get; set; } = "SUCCESS";
    }
}
