using RuriLib.Extensions;
using RuriLib.Legacy.Blocks;
using RuriLib.Logging;
using System.Collections.Generic;

namespace RuriLib.Legacy.Models
{
    /// <summary>
    /// Represents a set of keys that can be checked in a OR/AND fashion and modifies the status of the BotData if successful.
    /// </summary>
    public class KeyChain
    {
        /// <summary>The type of the KeyChain.</summary>
        public KeychainType Type { get; set; } = KeychainType.Success;

        /// <summary>The mode of the KeyChain.</summary>
        public KeychainMode Mode { get; set; } = KeychainMode.OR;

        /// <summary>The type of the KeyChain in case Custom is selected.</summary>
        public string CustomType { get; set; } = "CUSTOM";

        /// <summary>The collection of Keys in the KeyChain.</summary>
        public List<Key> Keys { get; set; } = new();

        /// <summary>
        /// Checks all the Keys in the KeyChain.
        /// </summary>
        public bool CheckKeys(LSGlobals ls)
        {
            var data = ls.BotData;

            switch (Mode)
            {
                case KeychainMode.OR:
                    foreach (var key in Keys)
                    {
                        if (key.CheckKey(ls))
                        {
                            data.Logger.Log(string.Format("Found 'OR' Key {0} {1} {2}",
                                BlockBase.ReplaceValues(key.LeftTerm, ls).TruncatePretty(20),
                                key.Comparer.ToString(),
                                BlockBase.ReplaceValues(key.RightTerm, ls)),
                                LogColors.White);
                            return true;
                        }
                    }
                    return false;

                case KeychainMode.AND:
                    foreach (var key in Keys)
                    {
                        if (!key.CheckKey(ls))
                        {
                            return false;
                        }
                        else
                        {
                            data.Logger.Log(string.Format("Found 'AND' Key {0} {1} {2}",
                                BlockBase.ReplaceValues(key.LeftTerm, ls).TruncatePretty(20),
                                key.Comparer.ToString(),
                                BlockBase.ReplaceValues(key.RightTerm, ls)),
                                LogColors.White);
                        }
                    }
                    return true;
            }
            return false;
        }
    }

    /// <summary>The returned status upon a successful check of the keys. If no KeyChain was valid, the original status won't be changed.</summary>        
    public enum KeychainType
    {
        Success,
        Failure,
        Ban,
        Retry,
        Custom
    }

    /// <summary>The mode in which the keys should be checked.</summary>
    public enum KeychainMode
    {
        OR,
        AND
    }
}
