using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Blocks.Functions.Dictionary
{
    [BlockCategory("Dictionary Functions", "Blocks for working with dictionaries", "#9acd32")]
    public static class Methods
    {
        [Block("Adds an item to the dictionary")]
        public static void AddKeyValuePair(BotData data, [Variable] Dictionary<string, string> dictionary, string key, string value)
        {
            dictionary.Add(key, value);
            data.Logger.LogHeader();
            data.Logger.Log($"Added ({key}, {value})", LogColors.YellowGreen);
        }

        [Block("Removes an item with a given key from the dictionary")]
        public static void RemoveByKey(BotData data, [Variable] Dictionary<string, string> dictionary, string key)
        {
            data.Logger.LogHeader();

            if (dictionary.Remove(key))
                data.Logger.Log($"Removed the item with key {key}", LogColors.YellowGreen);
            else
                data.Logger.Log($"Could not find an item with key {key}", LogColors.YellowGreen);
        }

        [Block("Gets a dictionary key by value (old <DICT{value}>)")]
        public static string GetKey(BotData data, [Variable] Dictionary<string, string> dictionary, string value)
        {
            var key = dictionary.FirstOrDefault(kvp => kvp.Value == value).Key;
            data.Logger.LogHeader();
            data.Logger.Log($"Got key: {key}", LogColors.YellowGreen);
            return key;
        }
    }
}
