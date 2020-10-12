using System;
using System.Collections.Generic;
using System.Text;

namespace RuriLib.Models.Blocks.Custom.Keycheck
{
    public class Keychain
    {
        public List<Key> Keys { get; set; } = new List<Key>();
        public KeychainMode Mode { get; set; } = KeychainMode.OR;
        public string ResultStatus { get; set; } = "SUCCESS";
    }
}
