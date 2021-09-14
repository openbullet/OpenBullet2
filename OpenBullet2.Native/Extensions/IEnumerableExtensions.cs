using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        {
            // Need to run the loop a few times otherwise sometimes it throws and says CLIPBRD_E_CANT_OPEN
            // https://stackoverflow.com/questions/68666/clipbrd-e-cant-open-error-when-setting-the-clipboard-from-net
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(string.Join(Environment.NewLine, items.Select(i => mapping(i))));
                    return;
                }
                catch (COMException ex)
                {
                    const uint CLIPBRD_E_CANT_OPEN = 0x800401D0;

                    if ((uint)ex.ErrorCode != CLIPBRD_E_CANT_OPEN)
                    {
                        throw;
                    }
                }

                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
