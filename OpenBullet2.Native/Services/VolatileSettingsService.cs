using OpenBullet2.Native.Helpers;
using RuriLib.Models.Blocks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace OpenBullet2.Native.Services
{
    public class VolatileSettingsService
    {
        public List<BlockDescriptor> RecentDescriptors { get; set; } = new();

        public void AddRecentDescriptor(BlockDescriptor descriptor)
        {
            if (RecentDescriptors.Contains(descriptor))
            {
                RecentDescriptors.Remove(descriptor);
            }

            RecentDescriptors.Insert(0, descriptor);
        }

        public Dictionary<string, ListViewSortInfo> ListViewSorting { get; set; } = new();

        public VolatileSettingsService()
        {
            ListViewSorting["configs"] = new();
            ListViewSorting["wordlists"] = new();
        }
    }

    public class ListViewSortInfo
    {
        public string By { get; set; }
        public ListSortDirection Direction { get; set; }
    }
}
