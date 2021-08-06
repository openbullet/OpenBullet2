using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace OpenBullet2.Native.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void SaveToFile<T>(this IEnumerable<T> items, string fileName, Func<T, string> mapping)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "The filename must not be empty");
            }

            File.WriteAllLines(fileName, items.Select(i => mapping(i)));
        }

        public static void CopyToClipboard<T>(this IEnumerable<T> items, Func<T, string> mapping) 
            => Clipboard.SetText(string.Join(Environment.NewLine, items.Select(i => mapping(i))));
    }
}
