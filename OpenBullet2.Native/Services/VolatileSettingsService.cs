using RuriLib.Models.Blocks;
using System.Collections.Generic;

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
    }
}
