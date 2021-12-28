using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Conditions.Comparisons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RuriLib.Blocks.Functions.List
{
    [BlockCategory("List Functions", "Blocks for working with lists of strings", "#9acd32")]
    public static class Methods
    {
        [Block("Counts the number of elements in the list")]
        public static int GetListLength(BotData data, [Variable] List<string> list)
        {
            var count = list.Count;
            data.Logger.LogHeader();
            data.Logger.Log($"The list has {count} elements", LogColors.YellowGreen);
            return count;
        }

        [Block("Joins all the strings in the list to create a single string with the given separator")]
        public static string JoinList(BotData data, [Variable] List<string> list, string separator = ",")
        {
            var joined = string.Join(separator, list);
            data.Logger.LogHeader();
            data.Logger.Log($"Joined string: {joined}", LogColors.YellowGreen);
            return joined;
        }

        [Block("Sorts a list alphabetically", extraInfo = "If the elements of the list are numeric values, set numeric to true")]
        public static void SortList(BotData data, [Variable] List<string> list, bool ascending = true, bool numeric = false)
        {
            if (numeric)
            {
                var nums = list.Select(e => double.Parse(e, CultureInfo.InvariantCulture)).ToList();
                nums.Sort();
                list.Clear();
                list.AddRange(nums.Select(e => e.ToString()));
            }
            else
            {
                list.Sort();
            }

            if (!ascending)
                list.Reverse();

            data.Logger.LogHeader();
            data.Logger.Log("Sorted list in " + (ascending ? "ascending" : "descending") + " order", LogColors.YellowGreen);
        }

        [Block("Concatenates two lists into a single one")]
        public static List<string> ConcatLists(BotData data, [Variable] List<string> list1, [Variable] List<string> list2)
        {
            var concat = list1.Concat(list2).ToList();
            data.Logger.LogHeader();
            data.Logger.Log("Concatenated the lists", LogColors.YellowGreen);
            return concat;
        }

        [Block("Zips two lists into a single one", 
            extraInfo = "You can specify the format for joined elements. " +
            "[0] is replaced with an element from the first list, and [1] with an element from the second list. " +
            "For example if the format is [0][1], the lists [1,2] and [a,b] will be zipped into [1a,2b]")]
        public static List<string> ZipLists(BotData data, [Variable] List<string> list1, [Variable] List<string> list2,
            bool fill = false, string fillString = "NULL", string format = "[0][1]")
        {
            List<string> zipped;
            Func<string, string, string> zipFunc = (a, b) => format.Replace("[0]", a).Replace("[1]", b);

            if (fill)
            {
                zipped = list1.Count < list2.Count
                    ? list1.Concat(Enumerable.Repeat(fillString, list2.Count - list2.Count)).Zip(list2, zipFunc).ToList()
                    : list1.Zip(list2.Concat(Enumerable.Repeat(fillString, list1.Count - list2.Count)), zipFunc).ToList();
            }
            else
            {
                zipped = list1.Zip(list2, zipFunc).ToList();
            }
            
            data.Logger.LogHeader();
            data.Logger.Log("Zipped the lists", LogColors.YellowGreen);
            return zipped;
        }

        [Block("Maps two lists into a dictionary", extraInfo = "For example [1,2] and [a,b] will be mapped into {(1,a), (2,b)}")]
        public static Dictionary<string, string> MapLists(BotData data, [Variable] List<string> list1, [Variable] List<string> list2)
        {
            var mapped = list1.Zip(list2, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            data.Logger.LogHeader();
            data.Logger.Log("Mapped the lists", LogColors.YellowGreen);
            return mapped;
        }

        [Block("Adds an item to a list",
            extraInfo = "If the index is negative, it will start from the end of the list. For example an index of -1 will add the item at the end of the list")]
        public static void AddToList(BotData data, [Variable] List<string> list, string item, int index = -1)
        {
            if (list.Count == 0) index = 0;
            else if (index < 0) index += list.Count + 1;
            list.Insert(index, item);

            data.Logger.LogHeader();
            data.Logger.Log($"Added {item} at index {index}", LogColors.YellowGreen);
        }

        [Block("Removes an item from a list",
            extraInfo = "If the index is negative, it will start from the end of the list. For example an index of -1 will remove the item at the end of the list")]
        public static void RemoveFromList(BotData data, [Variable] List<string> list, int index = 0)
        {
            if (list.Count == 0) index = 0;
            else if (index < 0) index += list.Count +1 ;
            var removedItem = list[index];
            list.RemoveAt(index);

            data.Logger.LogHeader();
            data.Logger.Log($"Removed item {removedItem} at index {index}", LogColors.YellowGreen);
        }

        [Block("Removes all items that match a given condition from a list")]
        public static void RemoveAllFromList(BotData data, [Variable] List<string> list, StrComparison comparison, string term)
        {
            var count = list.RemoveAll(i => RuriLib.Functions.Conditions.Conditions.Check(i, comparison, term));

            data.Logger.LogHeader();
            data.Logger.Log($"Removed {count} items", LogColors.YellowGreen);
        }

        [Block("Removes duplicate items from a list")]
        public static List<string> RemoveDuplicates(BotData data, [Variable] List<string> list)
        {
            var unique = list.Distinct().ToList();
            data.Logger.LogHeader();
            data.Logger.Log($"Removed {list.Count - unique.Count} duplicates", LogColors.YellowGreen);
            return unique;
        }

        [Block("Gets a random item from a list")]
        public static string GetRandomItem(BotData data, [Variable] List<string> list)
        {
            var item = list[data.Random.Next(list.Count)];
            data.Logger.LogHeader();
            data.Logger.Log($"Got random item: {item}", LogColors.YellowGreen);
            return item;
        }

        [Block("Shuffles the items of a list")]
        public static void Shuffle(BotData data, [Variable] List<string> list)
        {
            list.Shuffle(data.Random);
            data.Logger.LogHeader();
            data.Logger.Log("Shuffled the list", LogColors.YellowGreen);
        }

        [Block("Splits the items of a list to create a dictionary", name = "To Dictionary")]
        public static Dictionary<string, string> ListToDictionary(BotData data, [Variable] List<string> list,
            string separator = ":", bool autoTrim = true)
        {
            Dictionary<string, string> dict = new();

            foreach (var item in list)
            {
                var split = item.Split(separator, 2, autoTrim ? StringSplitOptions.TrimEntries : StringSplitOptions.None);

                if (!string.IsNullOrEmpty(split[0]))
                {
                    dict[split[0]] = split[1];
                }
            }

            data.Logger.LogHeader();
            data.Logger.Log("Split the list into a dictionary", LogColors.YellowGreen);
            return dict;
        }

        [Block("Gets the index of an element of a list", name = "Index Of")]
        public static int ListIndexOf(BotData data, [Variable] List<string> list, string item, bool exactMatch = false,
            bool caseSensitive = false)
        {
            var comparer = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var elem = list.FirstOrDefault(i => exactMatch
                ? i.Equals(item, comparer)
                : i.Contains(item, comparer));

            data.Logger.LogHeader();

            if (elem is null)
            {
                data.Logger.Log("Item not found in the list, returning -1", LogColors.YellowGreen);
                return -1;
            }
            else
            {
                var index = list.IndexOf(elem);
                data.Logger.Log($"Item found at index {index}", LogColors.YellowGreen);
                return index;
            }
        }

        [Block("Creates a list of N numbers starting from the specified number", name = "Create List of Numbers")]
        public static List<string> CreateListOfNumbers(BotData data, int start, int count)
        {
            data.Logger.LogHeader();

            var list = Enumerable.Range(start, count).Select(i => i.ToString()).ToList();

            data.Logger.Log($"Created a list of {count} numbers starting from {start}", LogColors.YellowGreen);

            return list;
        }
    }
}
