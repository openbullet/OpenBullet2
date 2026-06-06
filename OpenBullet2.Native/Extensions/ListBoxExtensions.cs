using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace OpenBullet2.Native.Extensions;

public static class ListBoxExtensions
{
    public static List<T> GetSelectedItemsInDisplayOrder<T>(this ListBox listBox)
    {
        if (listBox.SelectedItems.Count == 0)
        {
            return [];
        }

        var selectedItems = listBox.SelectedItems.Cast<object>().ToHashSet();

        return listBox.Items
            .Cast<object>()
            .Where(selectedItems.Contains)
            .Cast<T>()
            .ToList();
    }
}
